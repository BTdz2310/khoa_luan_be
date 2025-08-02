using learniverse_be.Models;

namespace learniverse_be.DTOs;

public class LivestreamCourseDto
{
  public LivestreamResponseDto Livestream { get; set; } = default!;
  public CourseResponseDTO Course { get; set; } = default!;
  public List<VideoChatResponseDto> Chats { get; set; } = default!;
}