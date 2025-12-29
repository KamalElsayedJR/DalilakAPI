using System.ComponentModel.DataAnnotations;

namespace Core.Entities.Auth
{
    public class EmailOtpToken
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(4)]
        public string OtpCode { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsUsed { get; set; }
        
        public DateTime? UsedAt { get; set; }
        
        public bool IsActive => !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}