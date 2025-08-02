using learniverse_be.Models;

namespace learniverse_be.DTOs;

public class LectureDto
{
  public LectureResponseDTO Lecture { get; set; } = default!;
  public Video? Video { get; set; }
}