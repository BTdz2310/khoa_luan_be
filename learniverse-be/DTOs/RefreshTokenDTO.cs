using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class RefreshTokenDto
{
  [Required]
  public string RefreshToken { get; set; } = default!;
}