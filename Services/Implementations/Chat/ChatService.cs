using System.Text;
using System.Text.Json;
using Core.DTOs.Chat;
using Core.Entities.Chat;
using Core.Interfaces.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services.Implementations.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;
        
        public ChatService(
            IChatRepository chatRepository, 
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task<Guid> StartNewSessionAsync(int userId)
        {
            var session = await _chatRepository.CreateSessionAsync(userId);
            return session.Id;
        }

        public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
        {
            try
            {
                Guid sessionId;
                if (request.SessionId == null)
                {
                    sessionId = await StartNewSessionAsync(request.UserId);
                }
                else
                {
                    var existingSession = await _chatRepository.GetSessionAsync(request.SessionId.Value);
                    
                    if (existingSession == null)
                    {
                        _logger.LogWarning("Session not found - UserId:{UserId}, SessionId:{SessionId}", 
                            request.UserId, request.SessionId);
                        
                        sessionId = await StartNewSessionAsync(request.UserId);
                    }
                    else if (existingSession.UserId != request.UserId)
                    {
                        _logger.LogError("Session ownership violation - RequestUserId:{UserId}, SessionOwner:{OwnerId}, SessionId:{SessionId}", 
                            request.UserId, existingSession.UserId, request.SessionId);
                        
                        return new ChatResponse
                        {
                            SessionId = request.SessionId.Value,
                            Messages = new List<string>(),
                            IsSuccess = false,
                            Error = "Invalid session access"
                        };
                    }
                    else
                    {
                        sessionId = request.SessionId.Value;
                    }
                }

                _logger.LogInformation("Processing chat message - UserId:{UserId}, SessionId:{SessionId}, MessageLength:{Length}", 
                    request.UserId, sessionId, request.Message.Length);

                var existingMessages = await _chatRepository.GetSessionMessagesAsync(sessionId);
                var isFirstMessage = existingMessages.Count == 0;

                await _chatRepository.AddMessageAsync(sessionId, "user", request.Message);

                if (isFirstMessage && !string.IsNullOrWhiteSpace(request.Message))
                {
                    var sessionName = request.Message.Length > 100 
                        ? request.Message.Substring(0, 100).Trim() 
                        : request.Message.Trim();
                    
                    await RenameSessionAsync(sessionId, sessionName, request.UserId);
                    
                    _logger.LogInformation("Auto-renamed session - SessionId:{SessionId}, Name:{Name}", 
                        sessionId, sessionName);
                }

                var aiResponse = await CallExternalAIAsync(request.Message, sessionId);
                
                var responseString = aiResponse is string str ? str : JsonSerializer.Serialize(aiResponse);
                await _chatRepository.AddMessageAsync(sessionId, "ai", responseString);

                return new ChatResponse
                {
                    SessionId = sessionId,
                    Messages = new List<string> { responseString },
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message - UserId:{UserId}, SessionId:{SessionId}", 
                    request.UserId, request.SessionId);
                
                return new ChatResponse
                {
                    SessionId = request.SessionId ?? Guid.Empty,
                    Messages = new List<string> { "I'm sorry, I encountered an error while processing your request. Please try again." },
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<List<ChatMessage>> GetSessionHistoryAsync(Guid sessionId)
        {
            return await _chatRepository.GetSessionMessagesAsync(sessionId);
        }
        
        public async Task<PaginatedResult<ChatSession>> GetUserSessionsAsync(int userId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            
            return await _chatRepository.GetUserSessionsAsync(userId, pageNumber, pageSize);
        }
        
        public async Task<PaginatedResult<ChatSession>> SearchUserChatSessionsAsync(int userId, string searchTerm, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetUserSessionsAsync(userId, pageNumber, pageSize);
            }

            return await _chatRepository.SearchUserChatSessionsAsync(userId, searchTerm.Trim(), pageNumber, pageSize);
        }
        
        public async Task<bool> RenameSessionAsync(Guid sessionId, string newName, int userId)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return false;

            return await _chatRepository.RenameSessionAsync(sessionId, newName.Trim(), userId);
        }
        
        public async Task<bool> DeleteSessionAsync(Guid sessionId, int userId)
        {
            return await _chatRepository.DeleteSessionAsync(sessionId, userId);
        }
        
        private async Task<object> CallExternalAIAsync(string message, Guid sessionId)
        {
            try
            {
                var apiEndpoint = _configuration["AiApi:Endpoint"];
                var apiKey = _configuration["AiApi:ApiKey"];

                if (string.IsNullOrEmpty(apiEndpoint))
                {
                    throw new InvalidOperationException("AI API configuration is missing");
                }

                var requestPayload = new
                {
                    message = message,
                    session_id = sessionId.ToString(),
                    context = "car recommendation system",
                    max_tokens = 500
                };

                _logger.LogDebug("Sending AI request with session - SessionId:{SessionId}, MessageLength:{Length}", 
                    sessionId, message.Length);

                var jsonContent = JsonSerializer.Serialize(requestPayload);
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiEndpoint)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrEmpty(apiKey))
                {
                    httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                }

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogDebug("AI response received - SessionId:{SessionId}, ResponseLength:{Length}", 
                        sessionId, responseContent.Length);

                    try
                    {
                        using var jsonDocument = JsonDocument.Parse(responseContent);
                        var transformedObject = TransformAnswerField(jsonDocument);
                        return transformedObject;
                    }
                    catch (JsonException)
                    {
                        return responseContent;
                    }
                }
                else
                {
                    _logger.LogWarning("AI API call failed - SessionId:{SessionId}, StatusCode:{StatusCode}", 
                        sessionId, response.StatusCode);
                    throw new HttpRequestException($"AI API call failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling external AI API - SessionId:{SessionId}", sessionId);
                throw;
            }
        }
        
        private Dictionary<string, object> TransformAnswerField(JsonDocument jsonDocument)
        {
            var rootElement = jsonDocument.RootElement;
            var result = new Dictionary<string, object>();
            
            foreach (var property in rootElement.EnumerateObject())
            {
                if (property.Name.Equals("answer", StringComparison.OrdinalIgnoreCase) && 
                    property.Value.ValueKind == JsonValueKind.String)
                {
                    var answerValue = property.Value.GetString();
                    if (!string.IsNullOrEmpty(answerValue))
                    {
                        var index = answerValue.IndexOf(':');
                        if (index != -1)
                        {
                            result[property.Name] = answerValue.Substring(0, index);
                        }
                        else
                        {
                            result[property.Name] = answerValue;
                        }
                    }
                    else
                    {
                        result[property.Name] = answerValue;
                    }
                }
                else
                {
                    result[property.Name] = ConvertJsonElement(property.Value);
                }
            }
            
            return result;
        }
        
        private object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        obj[prop.Name] = ConvertJsonElement(prop.Value);
                    }
                    return obj;
                case JsonValueKind.Array:
                    var array = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(ConvertJsonElement(item));
                    }
                    return array;
                default:
                    return element.ToString();
            }
        }
    }
}