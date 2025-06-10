namespace learniverse_be.Models;

public class ForgetPasswordResponseDto
{
  public int AuthId { get; set; } = default!;
  public Guid HashCode { get; set; } = default!;
}