namespace learniverse_be.Services.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileKey);
    string GetFileUrl(string fileKey);
    string GenerateUploadUrl(string filePath, TimeSpan validDuration, string contentType);
    string GenerateSignedUrl(string hlsDirectoryKey, int expireSeconds = 3600);
    public string GenerateUploadUrlWithAutoContentType(Guid liveId, string fileName);
}