namespace learniverse_be.Models;

public class UploadHlsVideoDto
{
  public string HlsMasterPlaylistUrl { get; set; } = default!;
  public string HlsDirectoryKey { get; set; } = default!;
}