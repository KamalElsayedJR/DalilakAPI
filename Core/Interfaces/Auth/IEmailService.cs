namespace Core.Interfaces.Auth
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetOtpAsync(string email, string otpCode);
        Task<bool> SendEmailVerificationAsync(string email, string otpCode);
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);
    }
}