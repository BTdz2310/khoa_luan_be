using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class CreateProfileDTO
{
  [Required]
  public string FullName { get; set; } = string.Empty;

  [Required]
  public DateOnly BirthDate { get; set; }

  [Required]
  public Gender Gender { get; set; }

  public string Bio { get; set; } = string.Empty;

  [Required]
  public List<int> Interestings { get; set; } = new();
}