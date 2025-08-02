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
  [Column(TypeName = "decimal(18,6)")]
  public decimal Order { get; set; }

  public bool IsPreviewable { get; set; } = false;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; }
  public Video? Video { get; set; } = default!;
}
