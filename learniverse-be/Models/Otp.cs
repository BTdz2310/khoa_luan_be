using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public enum OtpType
{
  Register = 1,
  ForgotPassword = 2,
  TwoFactor = 3
}

public class Otp
{
  [Key]
  public int Id { get; set; }

  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  public Guid HashCode { get; set; } = Guid.NewGuid();

  [Required]
  public string VerificationCode { get; set; } = string.Empty;

  [Required]
  public byte[] Salt { get; set; }

  [Required]
  public OtpType Type { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime ExpirationTime { get; set; }

  public bool IsUsed { get; set; } = false;

  [ForeignKey("Auth")]
  public int? AuthId { get; set; }

  public Auth Auth { get; set; } = default!;

  public Otp()
  {
    ExpirationTime = CreatedAt.AddMinutes(5);
  }
}
