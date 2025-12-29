using API.Responses.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIProxyController> _logger;

        public AIProxyController(HttpClient httpClient, IConfiguration configuration, ILogger<AIProxyController> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ProxyToAI([FromBody] object request)
        {
            try
            {
                var apiEndpoint = _configuration["AiApi:Endpoint"];
                var apiKey = _configuration["AiApi:ApiKey"];

                if (string.IsNullOrEmpty(apiEndpoint))
                {
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("AI API configuration is missing"));
                }

                var jsonContent = JsonSerializer.Serialize(request);
                
                // Create request-specific message instead of mutating shared headers
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

                    try
                    {
                        var jsonDocument = JsonDocument.Parse(responseContent);
                        var transformedObject = TransformAnswerField(jsonDocument);
                        return Ok(transformedObject);
                    }
                    catch (JsonException)
                    {
                        return Content(responseContent, "text/plain");
                    }
                }
                else
                {
                    _logger.LogWarning($"AI API call failed with status code: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, ApiResponse<object>.ErrorResponse(errorContent));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI proxy");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing the request"));
            }
        }

        [HttpPost("proxy")]
        public async Task<IActionResult> DirectProxy([FromBody] JsonElement request)
        {
            return await ProxyRequest(request);
        }

        [HttpGet("proxy")]
        public async Task<IActionResult> DirectProxyGet([FromQuery] string message)
        {
            var requestObject = new { message };
            return await ProxyRequest(JsonSerializer.SerializeToElement(requestObject));
        }

        private async Task<IActionResult> ProxyRequest(JsonElement request)
        {
            try
            {
                var apiEndpoint = _configuration["AiApi:Endpoint"];
                var apiKey = _configuration["AiApi:ApiKey"];

                if (string.IsNullOrEmpty(apiEndpoint))
                {
                    return StatusCode(500, ApiResponse<object>.ErrorResponse("AI API configuration is missing"));
                }

                var jsonContent = request.GetRawText();
                
                // Create request-specific message instead of mutating shared headers
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

                    try
                    {
                        var jsonDocument = JsonDocument.Parse(responseContent);
                        var transformedObject = TransformAnswerField(jsonDocument);
                        return Ok(transformedObject);
                    }
                    catch (JsonException)
                    {
                        return Content(responseContent, "text/plain");
                    }
                }
                else
                {
                    _logger.LogWarning($"AI API call failed with status code: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, ApiResponse<object>.ErrorResponse(errorContent));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI proxy");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing the request"));
            }
        }

        private object TransformAnswerField(JsonDocument jsonDocument)
        {
            var rootElement = jsonDocument.RootElement;
            
            // Convert to dictionary to manipulate
            var result = new Dictionary<string, object>();
            
            foreach (var property in rootElement.EnumerateObject())
            {
                if (property.Name.Equals("answer", StringComparison.OrdinalIgnoreCase) && 
                    property.Value.ValueKind == JsonValueKind.String)
                {
                    var answerValue = property.Value.GetString();
                    if (!string.IsNullOrEmpty(answerValue))
                    {
                        var colonIndex = answerValue.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            // Extract only the text before the first colon
                            result[property.Name] = answerValue.Substring(0, colonIndex);
                        }
                        else
                        {
                            // No colon found, return unchanged
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
                    // Preserve all other fields exactly as-is
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