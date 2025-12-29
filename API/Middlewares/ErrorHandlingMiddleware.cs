using System.Net;
using System.Text.Json;
using API.Responses.Auth;

namespace API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                UnauthorizedAccessException => new { 
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Response = ApiResponse.ErrorResponse("Unauthorized access")
                },
                ArgumentException => new {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Response = ApiResponse.ErrorResponse("Invalid argument", new List<string> { exception.Message })
                },
                InvalidOperationException => new {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Response = ApiResponse.ErrorResponse("Invalid operation", new List<string> { exception.Message })
                },
                _ => new {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Response = ApiResponse.ErrorResponse("An unexpected error occurred")
                }
            };

            context.Response.StatusCode = response.StatusCode;
            
            var jsonResponse = JsonSerializer.Serialize(response.Response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}