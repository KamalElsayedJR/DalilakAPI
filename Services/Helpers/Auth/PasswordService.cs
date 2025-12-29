using System.Text.RegularExpressions;
using Core.Interfaces.Auth;

namespace Services.Helpers.Auth
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\?]"))
                return false;

            return true;
        }

        public string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            var chars = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }
            
            return new string(chars);
        }
    }
}