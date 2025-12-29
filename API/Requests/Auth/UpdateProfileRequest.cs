using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace API.Requests.Auth
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        public IFormFile? ProfileImage { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? CurrentPassword { get; set; }
    }
}