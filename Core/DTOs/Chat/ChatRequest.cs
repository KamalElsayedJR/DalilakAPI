using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Chat
{
    public class ChatRequest
    {
        public Guid? SessionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "Message cannot be empty")]
        [MaxLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}