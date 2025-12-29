namespace Core.DTOs.Auth
{
    public class UpdateProfileResultDto
    {
        public UserDto User { get; set; } = null!;
        public bool RequiresEmailVerification { get; set; }
        public string? Message { get; set; }
    }
}
