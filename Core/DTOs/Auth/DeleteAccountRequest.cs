namespace Core.DTOs.Auth
{
    public class DeleteAccountRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
