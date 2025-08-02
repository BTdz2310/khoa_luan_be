using learniverse_be.DTOs;

namespace learniverse_be.Models;

public class VideoChatResponseDto
{
  public Guid Id { get; set; }

  public Guid? VideoId { get; set; }
  public Guid? LivestreamId { get; set; }

  public UserDto? User { get; set; }

  public InstructorResponseDto? Instructor { get; set; }

  public string Content { get; set; } = default!;

  public double? VideoTimestamp { get; set; }

  public DateTime CreatedAt { get; set; }

  public Guid? ParentId { get; set; }
}