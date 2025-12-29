using Core.Interfaces.Common;

namespace Services.Implementations.Common
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public FileService()
        {
            // Use relative path from current directory
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");
            _uploadPath = Path.Combine(wwwrootPath, "uploads", "profile");
            
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> SaveProfileImageAsync(Stream fileStream, string fileName, int userId)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("File is empty");

            if (fileStream.Length > MaxFileSize)
                throw new ArgumentException("File size exceeds maximum allowed size of 5MB");

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only JPG, PNG, and GIF files are allowed");

            // Generate unique filename: userId_timestamp_guid.ext
            var uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            // Return relative URL path
            return $"/uploads/profile/{uniqueFileName}";
        }

        public Task DeleteProfileImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return Task.CompletedTask;

            try
            {
                // Extract filename from URL (e.g., /uploads/profile/filename.jpg)
                if (imageUrl.StartsWith("/uploads/profile/"))
                {
                    var fileName = Path.GetFileName(imageUrl);
                    var filePath = Path.Combine(_uploadPath, fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch
            {
                // Silently fail - file might already be deleted or doesn't exist
            }

            return Task.CompletedTask;
        }
    }
}
