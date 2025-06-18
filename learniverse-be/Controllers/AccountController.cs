using System.Security.Claims;
using learniverse_be.Data;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("/api/account")]
public class AccountController(AppDbContext context, ILogger<AccountController> logger, IS3Service S3Service) : ControllerBase
{
  private readonly ILogger<AccountController> _logger = logger;
  private readonly AppDbContext _context = context;
  private readonly IS3Service _s3Service = S3Service;

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
        Avatar = auth.User.Avatar,
        Bio = auth.User.Bio,
        BirthDate = auth.User.BirthDate,
        Gender = auth.User.Gender
      }
    }, "Lấy thông tin thành công.", null));
  }

  [Authorize]
  [Consumes("multipart/form-data")]
  [HttpPost("profile")]
  public async Task<ActionResult<ApiResponse<object>>> CreateProfile([FromForm] CreateProfileDTO dto, IFormFile? file)
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return Unauthorized(new ApiResponse<object>(null, "Token không hợp lệ.", null));
    }

    var auth = await _context.Auths
        .Include(a => a.User)
        .FirstOrDefaultAsync(a => a.Id == authIdInt);

    if (auth == null || auth.User != null)
    {
      return NotFound(new ApiResponse<object>(null, "Không tìm thấy tài khoản đủ điều kiện thực hiện hành động.", null));
    }

    string? avatar = null;

    if (file != null)
    {
      using var stream = file.OpenReadStream();
      avatar = await _s3Service.UploadFileAsync(stream, $"avatars/{Guid.NewGuid()}_{file.FileName}", file.ContentType);
    }

    var user = new User
    {
      FullName = dto.FullName,
      Avatar = avatar,
      Bio = dto.Bio,
      BirthDate = dto.BirthDate,
      Gender = dto.Gender,
    };

    auth.User = user;

    await _context.SaveChangesAsync();

    return Ok(new ApiResponse<UserResponseDTO>(new UserResponseDTO
    {
      Id = auth.User.Id,
      FullName = auth.User.FullName,
      Avatar = auth.User.Avatar,
      Bio = auth.User.Bio,
      BirthDate = auth.User.BirthDate,
      Gender = auth.User.Gender
    }, "Tạo profile thành công.", null));

  }
}