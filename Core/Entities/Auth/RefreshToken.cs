using System.ComponentModel.DataAnnotations;

namespace Core.Entities.Auth
{
    public class RefreshToken
    {
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? RevokedAt { get; set; }
        
        public string? RevokedByIp { get; set; }
        
        public string? CreatedByIp { get; set; }
        
        public string? ReasonRevoked { get; set; }
        
        // Foreign Key
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}