namespace learniverse_be.Models;

public class AuthDto
{
  public int AuthId { get; set; }
  public string Username { get; set; } = default!;
  public string Email { get; set; } = default!;
  public UserDto? User { get; set; }
  public DateTime CreatedAt { get; set; }
}