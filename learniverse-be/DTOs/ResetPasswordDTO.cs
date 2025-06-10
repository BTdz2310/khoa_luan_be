using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class ResetPasswordDto
{
  [Required]
  public Guid HashCode { get; set; } = default!;

  [Required]
  public string VerificationCode { get; set; } = default!;

  [Required]
  public string NewPassword { get; set; } = default!;
}