using learniverse_be.DTOs;

namespace learniverse_be.Models;

public class CourseResponseDTO
{
  public Guid Id { get; set; }
  public CategoriesDto Category { get; set; } = default!;

  public string Title { get; set; } = string.Empty;

  public string Slug { get; set; } = string.Empty;

  public string ShortDescription { get; set; } = string.Empty;

  public Language Language { get; set; }

  public Level Level { get; set; }

  public decimal Price { get; set; }

  public List<string> Requirements { get; set; } = new();

  public List<string> LearningObjectives { get; set; } = new();

  public Status Status { get; set; }
  public InstructorResponseDto Instructor { get; set; } = default!;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  public string? Image { get; set; }
}