using learniverse_be.Models;

namespace learniverse_be.DTOs;

public class LectureModerationResponseDto
{
  public Guid? LectureId { get; set; }
  public ModerationAction ActionType { get; set; }

  public string NewData { get; set; } = string.Empty;

  public int InstructorId { get; set; }
}