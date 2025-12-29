using System.ComponentModel.DataAnnotations;

namespace Core.Entities.Chat
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid SessionId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string Sender { get; set; } = string.Empty; // "user" or "ai"
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ChatSession Session { get; set; } = null!;
    }
}