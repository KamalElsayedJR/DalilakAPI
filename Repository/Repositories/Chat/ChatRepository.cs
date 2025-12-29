using Core.Entities.Chat;
using Core.Interfaces.Chat;
using Core.DTOs.Chat;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository.Repositories.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly CarFinderDbContext _context;

        public ChatRepository(CarFinderDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession> CreateSessionAsync(int userId)
        {
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = $"Chat Session {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ChatSession?> GetSessionAsync(Guid sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<PaginatedResult<ChatSession>> GetUserSessionsAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<ChatSession>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResult<ChatSession>> SearchUserChatSessionsAsync(int userId, string searchTerm, int pageNumber, int pageSize)
        {
            var query = _context.ChatSessions
                .Where(s => s.UserId == userId && s.Name.ToLower().Contains(searchTerm.ToLower()))
                .OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<ChatSession>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> RenameSessionAsync(Guid sessionId, string newName, int userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null)
                return false;

            session.Name = newName;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsync(Guid sessionId, int userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null)
                return false;

            // Remove all messages first
            _context.ChatMessages.RemoveRange(session.Messages);
            
            // Remove the session
            _context.ChatSessions.Remove(session);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChatMessage> AddMessageAsync(Guid sessionId, string sender, string message)
        {
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Sender = sender,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        public async Task<List<ChatMessage>> GetSessionMessagesAsync(Guid sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateSessionAsync(ChatSession session)
        {
            _context.ChatSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SessionExistsAsync(Guid sessionId)
        {
            return await _context.ChatSessions.AnyAsync(s => s.Id == sessionId);
        }
    }
}