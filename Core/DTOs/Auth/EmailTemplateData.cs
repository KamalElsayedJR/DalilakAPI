namespace Core.DTOs.Auth
{
    public class PasswordResetOtpData
    {
        public string UserName { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "Dalilak";
        public int ExpirationMinutes { get; set; } = 5;
    }

    public class EmailVerificationData
    {
        public string UserName { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "Dalilak";
        public int ExpirationMinutes { get; set; } = 5;
    }

    public class WelcomeEmailData
    {
        public string UserName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "Dalilak";
        public string LoginUrl { get; set; } = string.Empty;
    }
}