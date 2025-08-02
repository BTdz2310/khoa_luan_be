namespace learniverse_be.DTOs;

public class LivestreamRequestDto
{
  public string Title { get; set; } = default!;
  public DateTimeOffset StartTime { get; set; }
  public DateTimeOffset EndTime { get; set; }
}