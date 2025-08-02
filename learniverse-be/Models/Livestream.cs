namespace learniverse_be.Models;

public enum LivestreamStatus
{
  Scheduled = 0,
  Live = 1,
  Finished = 2,
  Canceled = 3
}

public class Livestream
{
  public Guid Id { get; set; } = Guid.NewGuid();

  public Guid CourseId { get; set; }
  public Course Course { get; set; } = default!;

  public Guid StreamKey { get; set; } = Guid.NewGuid();

  public DateTimeOffset StartTime { get; set; }
  public DateTimeOffset EndTime { get; set; }

  public LivestreamStatus Status { get; set; } = LivestreamStatus.Scheduled;

  public string Title { get; set; } = default!;

  public string? PlaybackUrl { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
}

