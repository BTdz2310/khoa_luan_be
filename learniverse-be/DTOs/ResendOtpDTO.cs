using System.ComponentModel.DataAnnotations;
using learniverse_be.Models;

public class ResendOtpDto
{
  [Required]
  public int AuthId { get; set; }

  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  [Required]
  public OtpType Type { get; set; }
}
