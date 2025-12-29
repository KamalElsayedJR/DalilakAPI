using System.ComponentModel.DataAnnotations;

namespace API.Requests.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}