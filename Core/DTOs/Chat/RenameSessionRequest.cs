using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Chat
{
    public class RenameSessionRequest
    {
        [Required]
        [MaxLength(200)]
        public string NewName { get; set; } = string.Empty;
    }
}