using System.ComponentModel.DataAnnotations;

namespace Core.Entities.Auth
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [MaxLength(15)]
        public string? Phone { get; set; }
        
        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }
        
        public bool IsEmailVerified { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        // Password Reset OTP System - NEW PROPERTIES
        [MaxLength(4)]
        public string? PasswordResetOtp { get; set; }
        
        public DateTime? PasswordResetOtpExpiry { get; set; }
        
        public bool IsPasswordOtpVerified { get; set; } = false;
        
        // Email Change OTP System - NEW PROPERTIES
        [EmailAddress]
        [MaxLength(255)]
        public string? PendingEmail { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}