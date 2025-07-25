namespace learniverse_be.Models;

public class SectionResponseDto
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid CourseId { get; set; }
  public string Title { get; set; } = default!;
  public decimal Order { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; }
  public List<LectureResponseDTO> Lectures { get; set; } = new();
}