using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class ForgetPasswordDto
{
  [Required]
  [EmailAddress]
  public string Email { get; set; }
}