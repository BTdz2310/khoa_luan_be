using learniverse_be.DTOs;

namespace learniverse_be.Models;

public class LectureResponseDTO
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Title { get; set; } = default!;
  public string? Description { get; set; }
  public decimal Order { get; set; }
  public Guid SectionId { get; set; } = default!;
  public bool IsPreviewable { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public Status Status { get; set; }
}