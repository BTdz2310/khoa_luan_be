using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace learniverse_be.Models;

public class SectionRequestDto
{
  [Required]
  public Guid Id { get; set; } = default!;

  [Required]  
  public string Title { get; set; } = default!;

  [Required]
  public decimal Order { get; set; } = default!;

  [Required]
  public Guid CourseId { get; set; } = default!;
}