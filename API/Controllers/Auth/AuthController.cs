using System.Security.Claims;
using API.Requests.Auth;
using API.Responses.Auth;
using Core.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(
                    request.FullName,
                    request.Email,
                    request.Password);

                return Ok(ApiResponse<string>.SuccessResponse(
                    "Registration completed. Please verify your email using the OTP sent to you.",
                    "Registration completed. Please verify your email using the OTP sent to you."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred during registration"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.LoginAsync(request.Email, request.Password, ipAddress);

                return Ok(ApiResponse<object>.SuccessResponse(result, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login"));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<object>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                return Ok(ApiResponse<object>.SuccessResponse(result, "Token refreshed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during token refresh"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                await _authService.LogoutAsync(request.RefreshToken, ipAddress);

                return Ok(ApiResponse.SuccessResponse("Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during logout"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request.Email);
                
                if (!result)
                    return BadRequest(ApiResponse.ErrorResponse("User not found"));

                return Ok(ApiResponse.SuccessResponse("Password reset OTP has been sent to your email"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing your request"));
            }
        }

        // NEW ENDPOINT for OTP verification
        [HttpPost("verify-reset-otp")]
        public async Task<ActionResult<ApiResponse>> VerifyResetOtp([FromBody] VerifyResetOtpRequest request)
        {
            try
            {
                var result = await _authService.VerifyPasswordResetOtpAsync(request.Email, request.OtpCode);
                if (!result)
                    return BadRequest(ApiResponse.ErrorResponse("Invalid or expired OTP code"));

                return Ok(ApiResponse.SuccessResponse("OTP verified successfully. You can now reset your password"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP verification");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during OTP verification"));
            }
        }

        // UPDATED ENDPOINT - now uses email instead of token
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request.Email, request.Password, request.ConfirmPassword);
                if (!result)
                    return BadRequest(ApiResponse.ErrorResponse("Invalid request or OTP not verified"));

                return Ok(ApiResponse.SuccessResponse("Password reset successful"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during password reset"));
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                
                if (!result)
                    return BadRequest(ApiResponse.ErrorResponse("Failed to change password"));

                return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred during password change"));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _authService.GetCurrentUserAsync(userId);
                
                if (user == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

                return Ok(ApiResponse<object>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user information"));
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.UpdateUserProfileWithEmailAsync(
                    userId,
                    request.FullName,
                    request.Email,
                    request.ProfileImage,
                    request.CurrentPassword);

                return Ok(ApiResponse<object>.SuccessResponse(result, result.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating profile"));
            }
        }

        [HttpPost("send-email-verification")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> SendEmailVerification()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.SendEmailVerificationAsync(userId);
                
                if (!result)
                    return BadRequest(ApiResponse.ErrorResponse("Unable to send verification email"));

                return Ok(ApiResponse.SuccessResponse("Verification email sent successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while sending verification email"));
            }
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<ApiResponse<string>>> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(request.Email, request.OtpCode);
                
                if (!result)
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid or expired verification code"));

                return Ok(ApiResponse<string>.SuccessResponse("Email verified successfully", "Email verified successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return StatusCode(500, ApiResponse<string>.ErrorResponse("An error occurred during email verification"));
            }
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult<ApiResponse>> ResendOtp([FromBody] ResendOtpRequest request)
        {
            try
            {
                var user = await _authService.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return BadRequest(ApiResponse.ErrorResponse("User not found"));

                if (user.IsEmailVerified)
                {
                    // Email is verified, resend OTP for forgot password
                    await _authService.ForgotPasswordAsync(request.Email);
                    return Ok(ApiResponse.SuccessResponse("Password reset OTP sent successfully"));
                }
                else
                {
                    // Email is not verified, resend OTP for email verification
                    var result = await _authService.SendEmailVerificationAsync(user.Id);
                    
                    if (!result)
                        return BadRequest(ApiResponse.ErrorResponse("Unable to send verification code"));

                    return Ok(ApiResponse.SuccessResponse("Verification code sent successfully"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while sending verification code"));
            }
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> DeleteMyAccount([FromBody] DeleteAccountRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.DeleteMyAccountAsync(userId, request.CurrentPassword, request.ConfirmPassword);
                
                if (!result)
                    return BadRequest(ApiResponse.ErrorResponse("Failed to delete account"));

                return Ok(ApiResponse.SuccessResponse("Account deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account");
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting account"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("user_id")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token");

            return userId;
        }

        private string GetIpAddress()
        {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            return ipAddress ?? "Unknown";
        }
    }
}