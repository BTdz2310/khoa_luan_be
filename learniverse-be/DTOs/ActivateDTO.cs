using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class ActivateDto
{
  [Required]
  public int AuthId { get; set; } = default!;

  [Required]
  public Guid HashCode { get; set; } = default!;

  [Required]
  public string VerificationCode { get; set; } = string.Empty;
}