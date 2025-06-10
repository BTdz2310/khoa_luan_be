namespace learniverse_be.Models;

public class LoginResponseDto
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public AuthDto? Auth { get; set; }
}