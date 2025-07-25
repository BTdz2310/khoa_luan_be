namespace learniverse_be.Models;

public class SectionSDto
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid CourseId { get; set; }
  public string Title { get; set; } = default!;
  public decimal Order { get; set; }
  public List<LectureSDto> Lectures { get; set; } = default!;
}