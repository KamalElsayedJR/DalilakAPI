namespace Core.DTOs.Chat
{
    public class ChatResponse
    {
        public Guid SessionId { get; set; }
        public List<string> Messages { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? Error { get; set; }
    }
}