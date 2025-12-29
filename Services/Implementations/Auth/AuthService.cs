using Core.DTOs.Auth;
using Core.Entities.Auth;
using Core.Interfaces.Auth;
using Core.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using Services.Utilities;

namespace Services.Implementations.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;

        public AuthService(
            IAuthRepository authRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IEmailService emailService,
            IFileService fileService)
        {
            _authRepository = authRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _emailService = emailService;
            _fileService = fileService;
        }

        public async Task<AuthenticationResultDto> RegisterAsync(string fullName, string email, string password)
        {
            if (await _authRepository.EmailExistsAsync(email))
                throw new InvalidOperationException("Email already exists");

            if (!_passwordService.IsPasswordStrong(password))
                throw new ArgumentException("Password does not meet strength requirements");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = _passwordService.HashPassword(password),
                IsEmailVerified = false // EmailConfirmed remains false until verification
            };

            var createdUser = await _authRepository.CreateUserAsync(user);
            
            // Generate 4-digit OTP using OtpGenerator utility
            var otpCode = OtpGenerator.Generate4DigitOtp();
            var emailOtp = new EmailOtpToken
            {
                Email = user.Email,
                OtpCode = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5) // 5-minute expiration as required
            };

            await _authRepository.SaveEmailOtpAsync(emailOtp);
            
            // Send OTP to user's email
            _ = Task.Run(async () => await _emailService.SendEmailVerificationAsync(user.Email, otpCode));

            // Return success message without JWT tokens (user cannot login yet)
            return new AuthenticationResultDto
            {
                AccessToken = string.Empty, // No token until email is verified
                RefreshToken = string.Empty,
                ExpiresAt = DateTime.MinValue,
                User = MapToUserDto(createdUser),
                Message = "Registration completed. Please verify your email using the OTP sent to you."
            };
        }

        public async Task<AuthenticationResultDto> LoginAsync(string email, string password, string? ipAddress = null)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null || !_passwordService.VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _authRepository.UpdateUserAsync(user);

            return await GenerateAuthenticationResult(user, ipAddress);
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            var token = await _authRepository.GetRefreshTokenAsync(refreshToken);
            if (token == null || !token.IsActive)
                throw new UnauthorizedAccessException("Invalid refresh token");

            // Revoke old token
            await _authRepository.RevokeRefreshTokenAsync(token, ipAddress, "Replaced by new token");

            // Generate new tokens
            var accessToken = _jwtService.GenerateAccessToken(token.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddDays(7);

            // Save new refresh token
            await _authRepository.SaveRefreshTokenAsync(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = token.UserId,
                ExpiresAt = expiresAt,
                CreatedByIp = ipAddress
            });

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = _jwtService.GetTokenExpiration(accessToken)
            };
        }

        public async Task LogoutAsync(string refreshToken, string? ipAddress = null)
        {
            var token = await _authRepository.GetRefreshTokenAsync(refreshToken);
            if (token != null && token.IsActive)
            {
                await _authRepository.RevokeRefreshTokenAsync(token, ipAddress, "Logged out");
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false; // Don't reveal if email exists

            // Generate 4-digit OTP and set 5-minute expiry
            var otpCode = OtpGenerator.Generate4DigitOtp();
            user.PasswordResetOtp = otpCode;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);
            user.IsPasswordOtpVerified = false;

            await _authRepository.UpdateUserAsync(user);
            await _emailService.SendPasswordResetOtpAsync(user.Email, otpCode);

            return true;
        }

        public async Task<bool> VerifyPasswordResetOtpAsync(string email, string otpCode)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            // Check OTP and expiry
            if (user.PasswordResetOtp != otpCode || 
                user.PasswordResetOtpExpiry == null || 
                user.PasswordResetOtpExpiry < DateTime.UtcNow)
                return false;

            // Mark OTP as verified and immediately clear OTP fields
            user.IsPasswordOtpVerified = true;
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            await _authRepository.UpdateUserAsync(user);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
                throw new ArgumentException("Passwords do not match");

            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null || !user.IsPasswordOtpVerified)
                return false;

            if (!_passwordService.IsPasswordStrong(password))
                throw new ArgumentException("Password does not meet strength requirements");

            // Update password and clear verification flag
            user.PasswordHash = _passwordService.HashPassword(password);
            user.IsPasswordOtpVerified = false;

            await _authRepository.UpdateUserAsync(user);

            // Revoke all existing refresh tokens
            await _authRepository.RevokeAllUserRefreshTokensAsync(user.Id, reason: "Password reset");

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;

            if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            if (!_passwordService.IsPasswordStrong(newPassword))
                throw new ArgumentException("Password does not meet strength requirements");

            user.PasswordHash = _passwordService.HashPassword(newPassword);
            await _authRepository.UpdateUserAsync(user);

            // Revoke all existing refresh tokens except current session
            await _authRepository.RevokeAllUserRefreshTokensAsync(user.Id, reason: "Password changed");

            return true;
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<UserDto> UpdateUserProfileAsync(int userId, string? fullName = null, string? phone = null, string? profileImageUrl = null)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (!string.IsNullOrWhiteSpace(fullName))
                user.FullName = fullName;
            if (phone != null)
                user.Phone = phone;
            if (profileImageUrl != null)
                user.ProfileImageUrl = profileImageUrl;

            await _authRepository.UpdateUserAsync(user);
            return MapToUserDto(user);
        }

        public async Task<UpdateProfileResultDto> UpdateUserProfileWithEmailAsync(
            int userId, 
            string fullName, 
            string email, 
            IFormFile? profileImage, 
            string? currentPassword)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Update basic profile info
            user.FullName = fullName;

            // Handle profile image upload
            if (profileImage != null && profileImage.Length > 0)
            {
                try
                {
                    // Delete old profile image if exists
                    if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                    {
                        await _fileService.DeleteProfileImageAsync(user.ProfileImageUrl);
                    }

                    // Save new profile image
                    using (var stream = profileImage.OpenReadStream())
                    {
                        user.ProfileImageUrl = await _fileService.SaveProfileImageAsync(stream, profileImage.FileName, userId);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Profile image upload failed: {ex.Message}");
                }
            }

            bool emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
            
            if (emailChanged)
            {
                // Require current password for email change
                if (string.IsNullOrWhiteSpace(currentPassword))
                    throw new ArgumentException("Current password is required to change email");

                // Verify current password
                if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Current password is incorrect");

                // Check if new email is already in use
                if (await _authRepository.EmailExistsAsync(email))
                    throw new InvalidOperationException("Email already exists");

                // Generate OTP for new email
                var otpCode = OtpGenerator.Generate4DigitOtp();
                var emailOtp = new EmailOtpToken
                {
                    Email = email,
                    OtpCode = otpCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };

                await _authRepository.SaveEmailOtpAsync(emailOtp);
                
                // Store pending email
                user.PendingEmail = email;
                
                // Send OTP to new email
                _ = Task.Run(async () => await _emailService.SendEmailVerificationAsync(email, otpCode));
                
                await _authRepository.UpdateUserAsync(user);

                return new UpdateProfileResultDto
                {
                    User = MapToUserDto(user),
                    RequiresEmailVerification = true,
                    Message = "Profile updated. Please verify your new email using the OTP sent to it."
                };
            }

            await _authRepository.UpdateUserAsync(user);

            return new UpdateProfileResultDto
            {
                User = MapToUserDto(user),
                RequiresEmailVerification = false,
                Message = "Profile updated successfully"
            };
        }

        public async Task<bool> VerifyEmailAsync(string email, string otp)
        {
            // Validate OTP using EmailOtpToken entity
            var emailOtp = await _authRepository.GetEmailOtpAsync(email, otp);
            if (emailOtp == null || !emailOtp.IsActive)
                return false;

            // Check if this is a registration verification or email change verification
            // First try to find user by email (registration case)
            var user = await _authRepository.GetUserByEmailAsync(email);
            
            // If not found, check if it's a pending email (email change case)
            if (user == null)
            {
                user = await _authRepository.GetUserByPendingEmailAsync(email);
                if (user == null)
                    return false;
                
                // This is an email change verification
                // Delete OTP to prevent reuse
                await _authRepository.DeleteEmailOtpAsync(emailOtp);
                
                // Update user email and clear pending email
                user.Email = email;
                user.PendingEmail = null;
                user.IsEmailVerified = true;
                
                await _authRepository.UpdateUserAsync(user);
                
                return true;
            }

            // This is a registration verification
            // Delete OTP to prevent reuse
            await _authRepository.DeleteEmailOtpAsync(emailOtp);
            
            user.IsEmailVerified = true;
            await _authRepository.UpdateUserAsync(user);

            // Send welcome email after successful verification (only for registration)
            _ = Task.Run(async () => await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName));

            return true;
        }

        public async Task<bool> SendEmailVerificationAsync(int userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null || user.IsEmailVerified)
                return false;

            // Generate 4-digit OTP using OtpGenerator utility
            var otpCode = OtpGenerator.Generate4DigitOtp();
            var emailOtp = new EmailOtpToken
            {
                Email = user.Email,
                OtpCode = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await _authRepository.SaveEmailOtpAsync(emailOtp);
            await _emailService.SendEmailVerificationAsync(user.Email, otpCode);

            return true;
        }

        public async Task<bool> DeleteMyAccountAsync(int userId, string currentPassword, string confirmPassword)
        {
            if (currentPassword != confirmPassword)
                throw new ArgumentException("Passwords do not match");

            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;

            if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            // Revoke all refresh tokens before deletion
            await _authRepository.RevokeAllUserRefreshTokensAsync(user.Id, reason: "Account deleted");

            // Perform hard delete (cascade will handle related entities)
            await _authRepository.DeleteUserAsync(user);

            return true;
        }

        private async Task<AuthenticationResultDto> GenerateAuthenticationResult(User user, string? ipAddress = null)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _authRepository.SaveRefreshTokenAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            });

            return new AuthenticationResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _jwtService.GetTokenExpiration(accessToken),
                User = MapToUserDto(user)
            };
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}