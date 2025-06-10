using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService : IJwtTokenService
{
  private readonly JwtSettings _jwtSettings;

  public JwtTokenService(IOptions<JwtSettings> jwtSettings)
  {
    _jwtSettings = jwtSettings.Value;
  }

  public string GenerateAccessToken(IEnumerable<Claim> claims)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _jwtSettings.Issuer,
      audience: _jwtSettings.Audience,
      claims: claims,
      expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken(IEnumerable<Claim> claims)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var refreshTokenDurationByMinute = _jwtSettings.RefreshTokenExpirationDays * 24 * 60;

    var token = new JwtSecurityToken(
      issuer: _jwtSettings.Issuer,
      audience: _jwtSettings.Audience,
      claims: claims,
      expires: DateTime.Now.AddMinutes(refreshTokenDurationByMinute),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = false,
      ValidateIssuer = false,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
      ValidateLifetime = false
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    try
    {
      var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
      if (validatedToken is not JwtSecurityToken jwtToken ||
          !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        return null;

      return principal;
    }
    catch
    {
      return null;
    }
  }

}