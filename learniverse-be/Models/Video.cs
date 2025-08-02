using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using learniverse_be.Models;

public enum VideoProcessingStatus
{
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Failed = 3
}

public class Video
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  public Guid? LectureId { get; set; }
  public Lecture? Lecture { get; set; }

  public Guid? LivestreamId { get; set; }
  public Livestream? Livestream { get; set; }

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
