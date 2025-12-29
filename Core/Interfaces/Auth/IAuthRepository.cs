using Core.Entities.Auth;

namespace Core.Interfaces.Auth
{
    public interface IAuthRepository
    {
        // User operations
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByPendingEmailAsync(string pendingEmail);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task DeleteUserAsync(User user);

        // Refresh token operations
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken token);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? revokedByIp = null, string? reason = null);
        Task RevokeAllUserRefreshTokensAsync(int userId, string? revokedByIp = null, string? reason = null);

        // Email OTP operations (new)
        Task<EmailOtpToken> SaveEmailOtpAsync(EmailOtpToken emailOtp);
        Task<EmailOtpToken?> GetEmailOtpAsync(string email, string otp);
        Task MarkEmailOtpAsUsedAsync(EmailOtpToken emailOtp);
        Task DeleteEmailOtpAsync(EmailOtpToken emailOtp);
    }
}