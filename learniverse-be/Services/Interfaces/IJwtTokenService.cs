using System.Security.Claims;

namespace learniverse_be.Services.Interfaces;

public interface IJwtTokenService
{
  string GenerateAccessToken(IEnumerable<Claim> claims);
  string GenerateRefreshToken(IEnumerable<Claim> claims);
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}