using Core.DTOs.Auth;
using Microsoft.AspNetCore.Http;

namespace Core.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthenticationResultDto> RegisterAsync(string fullName, string email, string password);
        Task<AuthenticationResultDto> LoginAsync(string email, string password, string? ipAddress = null);
        Task<TokenDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task LogoutAsync(string refreshToken, string? ipAddress = null);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> VerifyPasswordResetOtpAsync(string email, string otpCode);
        Task<bool> ResetPasswordAsync(string email, string password, string confirmPassword);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<UserDto?> GetCurrentUserAsync(int userId);
        Task<UserDto> UpdateUserProfileAsync(int userId, string? fullName = null, string? phone = null, string? profileImageUrl = null);
        Task<UpdateProfileResultDto> UpdateUserProfileWithEmailAsync(int userId, string fullName, string email, IFormFile? profileImage, string? currentPassword);
        Task<bool> SendEmailVerificationAsync(int userId);
        Task<bool> VerifyEmailAsync(string email, string otpCode);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> DeleteMyAccountAsync(int userId, string currentPassword, string confirmPassword);
    }
}