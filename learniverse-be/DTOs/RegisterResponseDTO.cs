namespace learniverse_be.Models;

public class RegisterResponseDto
{
  public int AuthId { get; set; } = default!;
  public Guid HashCode { get; set; } = default!;
}