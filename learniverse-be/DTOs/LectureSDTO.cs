namespace learniverse_be.Models;

public class LectureSDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = default!;
  public decimal Order { get; set; }
  public Guid SectionId { get; set; } = default!;
  public bool IsPreviewable { get; set; }
  public Status Status { get; set; }
}