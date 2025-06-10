namespace learniverse_be.Models;

public class VerifyOtpResponse
{
  public bool IsValid { get; set; }
  public int? AuthId { get; set; }
}