using System.ComponentModel.DataAnnotations;

namespace learniverse_be.DTOs;

public class InstructorResponseDto
{
  public int Id { get; set; }
  public string DisplayName { get; set; } = default!;

  public string Headline { get; set; } = default!;

  public string? Bio { get; set; } = default!;
  public string? Avatar { get; set; }

  public List<string> Languages { get; set; } = new();

  public List<string> Expertise { get; set; } = new();

  public Degree Degree { get; set; }

  public int ExperienceYears { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  public bool IsApproved { get; set; } = false;
  public Status Status { get; set; }
}
