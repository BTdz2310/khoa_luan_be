using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class Auth
{
  [Key]
  public int Id { get; set; }

  [Required, MaxLength(100)]
  public string Username { get; set; } = default!;

  [Required]
  [EmailAddress]
  public string Email { get; set; } = default!;

  [Required]
  public string PasswordHash { get; set; } = default!;

  [Required]
  public byte[] Salt { get; set; }

  public User User { get; set; } = default!;
  public Instructor Instructor { get; set; } = default!;
  public Otp Otp { get; set; } = default!;

  public bool IsActive { get; set; } = false;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}