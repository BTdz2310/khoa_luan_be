namespace learniverse_be.Models;

public class VideoChatRequestDto
{
  public Guid? VideoId { get; set; }
  public Guid? LivestreamId { get; set; }
  public Role Role { get; set; }
  public string Content { get; set; } = default!;
  public double? VideoTimestamp { get; set; }

  public Guid? ParentId { get; set; }
}