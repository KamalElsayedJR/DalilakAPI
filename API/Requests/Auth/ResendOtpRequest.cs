using System.ComponentModel.DataAnnotations;

namespace API.Requests.Auth
{
    public class ResendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}