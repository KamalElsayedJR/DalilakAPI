using System.Security.Cryptography;

namespace Services.Utilities
{
    public static class OtpGenerator
    {
        public static string Generate4DigitOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            
            // Convert to 4-digit number (0000-9999)
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10000;
            return number.ToString("D4");
        }
    }
}