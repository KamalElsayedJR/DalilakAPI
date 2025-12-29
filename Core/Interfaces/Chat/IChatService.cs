using Core.DTOs.Chat;
using Core.Entities.Chat;

namespace Core.Interfaces.Chat
{
    public interface IChatService
    {
        Task<Guid> StartNewSessionAsync(int userId);
        Task<ChatResponse> SendMessageAsync(ChatRequest request);
        Task<List<ChatMessage>> GetSessionHistoryAsync(Guid sessionId);
        Task<PaginatedResult<ChatSession>> GetUserSessionsAsync(int userId, int pageNumber, int pageSize);
        Task<PaginatedResult<ChatSession>> SearchUserChatSessionsAsync(int userId, string searchTerm, int pageNumber, int pageSize);
        Task<bool> RenameSessionAsync(Guid sessionId, string newName, int userId);
        Task<bool> DeleteSessionAsync(Guid sessionId, int userId);
    }
}