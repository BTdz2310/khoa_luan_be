namespace learniverse_be.Models;

public class VerifyOTp
{
  public Guid HashCode { get; set; } = default!;
  public string VerificationCode { get; set; } = string.Empty;
}