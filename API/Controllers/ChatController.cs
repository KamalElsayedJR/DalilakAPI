using API.Responses.Auth;
using Core.DTOs.Chat;
using Core.Interfaces.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly JsonSerializerOptions _historyJsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartChatSession()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized chat session start attempt - no valid user ID in claims");
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                var sessionId = await _chatService.StartNewSessionAsync(userId);
                
                _logger.LogInformation("Chat session started - UserId:{UserId}, SessionId:{SessionId}", userId, sessionId);
                
                return Ok(ApiResponse<object>.SuccessResponse(new { sessionId }, "Chat session started successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting chat session - UserId:{UserId}", GetUserIdFromClaims());
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while starting the chat session"));
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized send message attempt - no valid user ID in claims");
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                request.UserId = userId;
                
                _logger.LogInformation("Sending chat message - UserId:{UserId}, SessionId:{SessionId}, MessageLength:{Length}", 
                    userId, request.SessionId, request.Message.Length);

                var response = await _chatService.SendMessageAsync(request);
                
                if (!response.IsSuccess)
                {
                    _logger.LogWarning("Chat message failed - UserId:{UserId}, SessionId:{SessionId}, Error:{Error}", 
                        userId, response.SessionId, response.Error);
                    return StatusCode(500, ApiResponse<object>.ErrorResponse(response.Error ?? "An error occurred while processing your message"));
                }

                var aiRawResponse = response.Messages.FirstOrDefault() ?? string.Empty;

                try
                {
                    var jsonDocument = JsonDocument.Parse(aiRawResponse);
                    var jsonObject = JsonSerializer.Deserialize<object>(aiRawResponse);
                    
                    _logger.LogInformation("Chat message completed successfully - UserId:{UserId}, SessionId:{SessionId}", 
                        userId, response.SessionId);
                    
                    return Ok(ApiResponse<object>.SuccessResponse(jsonObject, "Message sent successfully"));
                }
                catch (JsonException)
                {
                    _logger.LogInformation("Chat message completed with plain text response - UserId:{UserId}, SessionId:{SessionId}", 
                        userId, response.SessionId);
                    return Ok(ApiResponse<object>.SuccessResponse(aiRawResponse, "Message sent successfully"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message - UserId:{UserId}, SessionId:{SessionId}", 
                    GetUserIdFromClaims(), request?.SessionId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while sending the message"));
            }
        }

        [HttpGet("history/{sessionId:guid}")]
        public async Task<IActionResult> GetChatHistory(Guid sessionId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                var messages = await _chatService.GetSessionHistoryAsync(sessionId);
                
                var formattedMessages = messages.Select(m => new
                {
                    id = m.Id,
                    sender = m.Sender,
                    message = ParseMessageForHistory(m.Message, m.Sender),
                    data = ExtractDataFromMessage(m.Message, m.Sender),
                    createdAt = m.CreatedAt
                }).ToList();

                return Ok(ApiResponse<object>.SuccessResponse(new { sessionId, messages = formattedMessages }, "Chat history retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat history for session {SessionId}", sessionId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving chat history"));
            }
        }

        /// <summary>
        /// Parses JSON-stringified AI messages and extracts readable text.
        /// User messages are returned as-is.
        /// </summary>
        private string ParseMessageForHistory(string message, string sender)
        {
            // User messages are always plain text
            if (sender?.ToLower() == "user")
                return message;

            // For AI messages, check if it's JSON-stringified
            if (string.IsNullOrWhiteSpace(message))
                return message;

            var trimmed = message.Trim();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                return message;

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;
                
                if (root.ValueKind == JsonValueKind.Object)
                {
                    // Try common message field names
                    if (root.TryGetProperty("message", out var messageProp) && 
                        messageProp.ValueKind == JsonValueKind.String)
                    {
                        return messageProp.GetString() ?? message;
                    }
                    
                    if (root.TryGetProperty("answer", out var answerProp) && 
                        answerProp.ValueKind == JsonValueKind.String)
                    {
                        return answerProp.GetString() ?? message;
                    }
                    
                    if (root.TryGetProperty("text", out var textProp) && 
                        textProp.ValueKind == JsonValueKind.String)
                    {
                        return textProp.GetString() ?? message;
                    }
                }
                
                return message;
            }
            catch (JsonException)
            {
                return message;
            }
        }

        /// <summary>
        /// Extracts data object (e.g., cars array) from JSON-stringified AI messages.
        /// Returns null for user messages or if no data is found.
        /// </summary>
        private object ExtractDataFromMessage(string message, string sender)
        {
            // User messages don't have data
            if (sender?.ToLower() == "user")
                return null;

            if (string.IsNullOrWhiteSpace(message))
                return null;

            var trimmed = message.Trim();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;
                
                if (root.ValueKind == JsonValueKind.Object)
                {
                    // Look for cars array
                    if (root.TryGetProperty("cars", out var carsProp))
                    {
                        return new { cars = JsonSerializer.Deserialize<object>(carsProp.GetRawText(), _historyJsonOptions) };
                    }
                    
                    // Look for general data field
                    if (root.TryGetProperty("data", out var dataProp))
                    {
                        return JsonSerializer.Deserialize<object>(dataProp.GetRawText(), _historyJsonOptions);
                    }
                }
                
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetUserChatSessions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                var result = await _chatService.GetUserSessionsAsync(userId, pageNumber, pageSize);
                
                var formattedSessions = result.Items.Select(s => new
                {
                    id = s.Id,
                    userId = s.UserId,
                    name = s.Name,
                    createdAt = s.CreatedAt,
                    isCompleted = s.IsCompleted,
                    completedAt = s.CompletedAt
                }).ToList();

                var data = new 
                { 
                    sessions = formattedSessions,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                };

                return Ok(ApiResponse<object>.SuccessResponse(data, "User sessions retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions for current user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user sessions"));
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUserChatSessions(
            [FromQuery] string searchTerm = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                var result = await _chatService.SearchUserChatSessionsAsync(userId, searchTerm, pageNumber, pageSize);
                
                var formattedSessions = result.Items.Select(s => new
                {
                    id = s.Id,
                    userId = s.UserId,
                    name = s.Name,
                    createdAt = s.CreatedAt,
                    isCompleted = s.IsCompleted,
                    completedAt = s.CompletedAt
                }).ToList();

                var data = new 
                { 
                    sessions = formattedSessions,
                    searchTerm,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                };

                return Ok(ApiResponse<object>.SuccessResponse(data, "Sessions searched successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching sessions for user with term '{SearchTerm}'", searchTerm);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while searching user sessions"));
            }
        }

        [HttpPut("rename/{sessionId:guid}")]
        public async Task<IActionResult> RenameSession(Guid sessionId, [FromBody] RenameSessionRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                var success = await _chatService.RenameSessionAsync(sessionId, request.NewName, userId);
                
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Session not found or you don't have permission to rename it"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(new { sessionId, newName = request.NewName }, "Session renamed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming session {SessionId}", sessionId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while renaming the session"));
            }
        }

        [HttpDelete("{sessionId:guid}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == 0)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                var success = await _chatService.DeleteSessionAsync(sessionId, userId);
                
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Session not found or you don't have permission to delete it"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(new { sessionId }, "Session deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the session"));
            }
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                              User.FindFirst("user_id")?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }
}