using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public class Course
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [ForeignKey("Category")]
  public int CategoryId { get; set; }
  public Category Category { get; set; } = default!;

  [ForeignKey("Instructor")]
  public int InstructorId { get; set; }
  public Instructor Instructor { get; set; } = default!;

  [Required]
  [MaxLength(255)]
  public string Title { get; set; } = default!;

  [Required]
  [MaxLength(255)]
  public string Slug { get; set; } = default!;

  [MaxLength(100)]
  public string Level { get; set; } = default!;

  [MaxLength(1000)]
  public string ShortDescription { get; set; } = default!;

  [MaxLength(100)]
  public string Language { get; set; } = default!;

  [Required]
  public Status Status { get; set; } = default!;

  [Range(0, double.MaxValue)]
  public decimal Price { get; set; }

  public List<string> Requirements { get; set; } = new List<string>();

  public List<string> LearningObjectives { get; set; } = new List<string>();
}