using learniverse_be.Models;

namespace learniverse_be.DTOs;

public class LectureModerationRequestDto
{
  public LectureRequestDto Lecture { get; set; } = default!;
  public Video? Video { get; set; }
}