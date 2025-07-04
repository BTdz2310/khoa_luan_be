using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public class Category
{
  [Key]
  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public string Code { get; set; } = null!;

  [Required]
  [MaxLength(255)]
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public string? IconUrl { get; set; }
  public string? Color { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  public ICollection<UserCategory> UserCategories { get; set; } = default!;

  public List<Course> Courses { get; set; } = new();
}
