using System.ComponentModel.DataAnnotations;

namespace Core.Entities.Chat
{
    public class ChatSession
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public int UserId { get; set; } // Changed from Guid to int
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsCompleted { get; set; } = false;
        
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}