namespace Core.DTOs.Auth
{
    public class AuthenticationResultDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
        public string? Message { get; set; } // Add message property for registration response
    }
}