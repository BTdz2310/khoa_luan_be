namespace learniverse_be.Models;

public class RefreshTokenResponseDto
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
}