using learniverse_be.Services.Interfaces;
using Amazon.S3;
using Amazon.S3.Model;

public class S3Service : IS3Service
{
  private readonly IAmazonS3 _s3Client;
  private readonly string _bucketName;

  public S3Service(IConfiguration configuration, IAmazonS3 s3Client)
  {
    _s3Client = s3Client;
    _bucketName = configuration["AWS:BucketName"];
  }

  public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
  {
    var request = new PutObjectRequest
    {
      BucketName = _bucketName,
      Key = fileName,
      InputStream = fileStream,
      ContentType = contentType,
    };

    var response = await _s3Client.PutObjectAsync(request);
    Console.WriteLine(GetFileUrl(fileName));
    return GetFileUrl(fileName);
  }

  public Task DeleteFileAsync(string fileKey) =>
    _s3Client.DeleteObjectAsync(_bucketName, fileKey);

  public string GetFileUrl(string fileKey) =>
    $"https://{_bucketName}.s3.amazonaws.com/{fileKey}";
}
