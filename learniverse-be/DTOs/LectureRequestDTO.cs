using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace learniverse_be.Models;

public class LectureRequestDto
{
  [Required]
  public Guid Id { get; set; } = default!;

  [Required]
  public string Title { get; set; } = default!;

  [Required]
  public decimal Order { get; set; } = default!;

  [Required]
  public Guid SectionId { get; set; } = default!;

  public bool IsPreviewable { get; set; } = false;
  public string? Description { get; set; }
}