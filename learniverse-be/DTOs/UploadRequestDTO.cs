namespace learniverse_be.Models;

public class UploadRequestDto
{
  public string OriginalFileName { get; set; } = default!;
  public string OriginalMimeType { get; set; } = default!;
  public long FileSize { get; set; }
  public int DurationSeconds { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }
}