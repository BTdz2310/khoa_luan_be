using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class LoginDto
{
  [Required]
  public string Username { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;
}