namespace Core.Interfaces.Common
{
    public interface IFileService
    {
        Task<string> SaveProfileImageAsync(Stream fileStream, string fileName, int userId);
        Task DeleteProfileImageAsync(string imageUrl);
    }
}
