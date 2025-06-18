namespace learniverse_be.Services.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileKey);
    string GetFileUrl(string fileKey);
}