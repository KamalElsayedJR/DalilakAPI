using System.ComponentModel.DataAnnotations;

namespace API.Requests.Auth
{
    public class VerifyResetOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string OtpCode { get; set; } = string.Empty;
    }
}