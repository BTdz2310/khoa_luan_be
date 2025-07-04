using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public class Instructor
{
  [Key]
  public int Id { get; set; }

  [Required, MaxLength(150)]
  public string DisplayName { get; set; } = default!;

  [Required, MaxLength(150)]
  public string Headline { get; set; } = default!;

  [ForeignKey("Auth")]
  public int AuthId { get; set; }
  public Auth Auth { get; set; } = default!;

  public string Bio { get; set; } = default!;

  public string? Avatar { get; set; }

  public List<string> Languages { get; set; } = new List<string>();
  public List<string> Expertise { get; set; } = new List<string>();
  public Degree Degree { get; set; }
  public int ExperienceYears { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  public Status Status { get; set; }

  public List<Course> Courses { get; set; } = new();
}