namespace learniverse_be.DTOs;

public class VideoResponseDto
{
  public Guid Id { get; set; }

  public Guid? LectureId { get; set; }

  public Guid? LivestreamId { get; set; }

  public string OriginalFileName { get; set; } = default!;
  public string OriginalFileKey { get; set; } = default!;
  public string OriginalMimeType { get; set; } = default!;
  public long FileSize { get; set; }

  public string? HlsMasterPlaylistUrl { get; set; } = default!;
  public string? HlsDirectoryKey { get; set; } = default!;

  public int? DurationSeconds { get; set; }
  public int? Width { get; set; }
  public int? Height { get; set; }

  public VideoProcessingStatus Status { get; set; } = VideoProcessingStatus.Pending;
  public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
