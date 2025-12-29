using System.ComponentModel.DataAnnotations;

namespace API.Requests.Auth
{
    public class VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "OTP code must be exactly 4 digits")]
        public string OtpCode { get; set; } = string.Empty;
    }
}