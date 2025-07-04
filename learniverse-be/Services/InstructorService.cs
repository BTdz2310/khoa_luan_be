using System.Net;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace learniverse_be.Services;

public class InstructorService : IInstructorService
{
  private readonly AppDbContext _context;
  private readonly IS3Service _s3Service;
  public InstructorService(AppDbContext context, IS3Service s3Service)
  {
    _context = context;
    _s3Service = s3Service;
  }

  public async Task<ApiResponse<InstructorResponseDto>> CreateInstructorAsync(int authId, CreateInstructorDto dto, IFormFile? file)
  {
    var auth = await _context.Auths
      .Include(a => a.User)
      .FirstOrDefaultAsync(a => a.Id == authId);

    if (auth == null)
    {
      return ApiResponse<InstructorResponseDto>.Error("Không tìm thấy tài khoản.", (int)HttpStatusCode.Unauthorized);
    }

    if (auth.Instructor != null)
    {
      return ApiResponse<InstructorResponseDto>.Error("Tài khoản đã tạo giảng viên rồi.", (int)HttpStatusCode.BadRequest);
    }

    string? avatar = null;

    if (file != null)
    {
      using var stream = file.OpenReadStream();
      avatar = await _s3Service.UploadFileAsync(stream, $"avatars/{Guid.NewGuid()}_{file.FileName}", file.ContentType);
    }

    var instructor = new Instructor
    {
      DisplayName = dto.DisplayName,
      Headline = dto.Headline,
      Avatar = avatar,
      Bio = dto.Bio ?? "",
      Languages = dto.Languages,
      Expertise = dto.Expertise,
      Degree = dto.Degree,
      ExperienceYears = dto.ExperienceYears,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
      Status = Status.Pending,
    };
    auth.Instructor = instructor;
    Console.WriteLine($"1: {instructor.Degree} 2. {dto.Degree} 3. {auth.Instructor}");
    _context.Instructors.Add(instructor);
    await _context.SaveChangesAsync();

    return ApiResponse<InstructorResponseDto>.Success(new InstructorResponseDto
    {
      Id = instructor.Id,
      DisplayName = instructor.DisplayName,
      Headline = instructor.Headline,
      Avatar = instructor.Avatar,
      Bio = instructor.Bio,
      Languages = instructor.Languages,
      Expertise = instructor.Expertise,
      ExperienceYears = instructor.ExperienceYears,
      CreatedAt = instructor.CreatedAt,
      UpdatedAt = instructor.UpdatedAt,
      Status = instructor.Status,
      Degree = instructor.Degree,
    }, "Đăng ký thành công giảng viên.", (int) HttpStatusCode.Created);
  }
}