using System.Security.Claims;
using learniverse_be.Data;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("/api/account")]
public class AuthController(AppDbContext context, ILogger<AuthController> logger, IJwtTokenService jwtTokenService, IMailService mailService) : ControllerBase
{
  private readonly ILogger<AuthController> _logger = logger;
  private readonly AppDbContext _context = context;
  private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
  private readonly IMailService _mailService = mailService;

  [Authorize]
  [HttpGet("profile")]
  public async Task<ActionResult<ApiResponse<object>>> GetProfile()
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return Unauthorized(new ApiResponse<object>(null, "Token không hợp lệ.", null));
    }

    var auth = await _context.Auths
        .Include(a => a.User)
        .FirstOrDefaultAsync(a => a.Id == authIdInt);

    if (auth == null)
    {
      return NotFound(new ApiResponse<object>(null, "Không tìm thấy tài khoản.", null));
    }

    return Ok(new ApiResponse<AuthDto>(new AuthDto
    {
      AuthId = auth.Id,
      Username = auth.Username,
      Email = auth.Email,
      CreatedAt = auth.CreatedAt,
      User = auth.User == null ? null : new UserDto
      {
        UserId = auth.User.Id,
        FullName = auth.User.FullName,
      }
    }, "Lấy thông tin thành công.", null));
  }
}