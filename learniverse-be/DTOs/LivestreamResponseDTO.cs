using learniverse_be.Models;

namespace learniverse_be.DTOs;

public class LivestreamResponseDto
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public CourseResponseDTO Course { get; set; } = default!;

  public Guid StreamKey { get; set; } = Guid.NewGuid();

  public DateTimeOffset StartTime { get; set; }
  public DateTimeOffset EndTime { get; set; }

  public LivestreamStatus Status { get; set; }

  public string Title { get; set; } = default!;

  public string? PlaybackUrl { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
}

