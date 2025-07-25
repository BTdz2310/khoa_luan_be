namespace learniverse_be.Models;

public class UploadRequestDto
{
  public string OriginalFileName { get; set; } = default!;
  public string OriginalMimeType { get; set; } = default!;
  public long FileSize { get; set; }
}