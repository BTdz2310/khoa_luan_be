namespace learniverse_be.Models;

public class UploadHlsVideoDto
{
  public string HlsMasterPlaylistUrl { get; set; } = default!;
  public string HlsDirectoryKey { get; set; } = default!;
  public int DurationSeconds { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }
}