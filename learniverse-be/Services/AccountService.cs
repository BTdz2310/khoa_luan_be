using System.Net;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AccountService : IAccountService
{

  private readonly AppDbContext _context;
  private readonly ILogger<AuthService> _logger;
  private readonly IS3Service _s3Service;

  public AccountService(AppDbContext context, ILogger<AuthService> logger, IS3Service s3Service)
  {
    _context = context;
    _logger = logger;
    _s3Service = s3Service;
  }

  public async Task<ApiResponse<AuthDto>> GetProfileAsync(int authId)
  {
    var auth = await _context.Auths
        .Include(a => a.User)
        .Include(a => a.Instructor)
        .FirstOrDefaultAsync(a => a.Id == authId);

    if (auth == null)
    {
      return ApiResponse<AuthDto>.Error("Không tìm thấy tài khoản.", (int)HttpStatusCode.NotFound);
    }

    return ApiResponse<AuthDto>.Success(new AuthDto
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
      },
      Instructor = auth.Instructor == null ? null : new InstructorResponseDto
      {
        Id = auth.Instructor.Id,
        DisplayName = auth.Instructor.DisplayName,
        Headline = auth.Instructor.Headline,
        Avatar = auth.Instructor.Avatar,
        Bio = auth.Instructor.Bio,
        Languages = auth.Instructor.Languages,
        Expertise = auth.Instructor.Expertise,
        ExperienceYears = auth.Instructor.ExperienceYears,
        CreatedAt = auth.Instructor.CreatedAt,
        UpdatedAt = auth.Instructor.UpdatedAt,
        Status = auth.Instructor.Status,
        Degree = auth.Instructor.Degree,
      },
    }, "Lấy thông tin thành công.");
  }

  public async Task<ApiResponse<UserResponseDTO>> CreateProfileAsync(int authId, CreateProfileDTO dto, IFormFile? file)
  {
    var auth = await _context.Auths
        .Include(a => a.User)
        .FirstOrDefaultAsync(a => a.Id == authId);

    if (auth == null || auth.User != null)
    {
      return ApiResponse<UserResponseDTO>.Error("Không tìm thấy tài khoản đủ điều kiện thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
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

    return ApiResponse<UserResponseDTO>.Success(new UserResponseDTO
    {
      Id = auth.User.Id,
      FullName = auth.User.FullName,
      Avatar = auth.User.Avatar,
      Bio = auth.User.Bio,
      BirthDate = auth.User.BirthDate,
      Gender = auth.User.Gender
    }, "Tạo profile thành công.");
  }
}