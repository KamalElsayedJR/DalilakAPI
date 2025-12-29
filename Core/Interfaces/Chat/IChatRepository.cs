using Core.Entities.Chat;
using Core.DTOs.Chat;

namespace Core.Interfaces.Chat
{
    public interface IChatRepository
    {
        Task<ChatSession> CreateSessionAsync(int userId);
        Task<ChatSession?> GetSessionAsync(Guid sessionId);
        Task<PaginatedResult<ChatSession>> GetUserSessionsAsync(int userId, int pageNumber, int pageSize);
        Task<PaginatedResult<ChatSession>> SearchUserChatSessionsAsync(int userId, string searchTerm, int pageNumber, int pageSize);
        Task<bool> RenameSessionAsync(Guid sessionId, string newName, int userId);
        Task<bool> DeleteSessionAsync(Guid sessionId, int userId);
        Task<ChatMessage> AddMessageAsync(Guid sessionId, string sender, string message);
        Task<List<ChatMessage>> GetSessionMessagesAsync(Guid sessionId);
        Task UpdateSessionAsync(ChatSession session);
        Task<bool> SessionExistsAsync(Guid sessionId);
    }
}