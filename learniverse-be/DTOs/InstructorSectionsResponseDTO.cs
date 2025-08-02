
using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class InstructorSectionsResponseDTO
{
  public CourseResponseDTO Course { get; set; } = default!;

  public List<SectionResponseDto> Sections { get; set; } = default!;
}