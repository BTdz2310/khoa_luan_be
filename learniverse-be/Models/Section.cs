using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public class Section
{
  [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey("Course")]
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = default!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = default!;

    [Required]
    public int Order { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public List<Lecture> Lectures { get; set; } = new();
}