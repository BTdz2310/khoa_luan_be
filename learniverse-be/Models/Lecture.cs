using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using learniverse_be.Models;

public class Lecture
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [ForeignKey("Section")]
  public Guid SectionId { get; set; }
  public Section Section { get; set; } = default!;

  [Required]
  [MaxLength(255)]
  public string Title { get; set; } = default!;

  [MaxLength(1000)]
  public string? Description { get; set; }

  [Required]
  public int Order { get; set; }

  public bool IsPreviewable { get; set; } = false;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
