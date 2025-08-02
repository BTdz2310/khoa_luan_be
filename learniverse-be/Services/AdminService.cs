using System.Net;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace learniverse_be.Services;

public class AdminService : IAdminService
{
  private readonly AppDbContext _context;
  public readonly ICourseService _courseService;

  public AdminService(AppDbContext context, ICourseService courseService)
  {
    _context = context;
    _courseService = courseService;
  }

  // private async Task<LectureResponseDTO> CreateLectureAsync(Lecture lecture)
  // {

  // }

  public async Task<ApiResponse<LectureModeration>> AcceptLectureRequestAsync(Guid lectureRequestId, int adminId)
  {
    var lectureModeration = await _context.LectureModerations.FirstOrDefaultAsync(l => l.Id == lectureRequestId);

    if (lectureModeration == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    var dto = JsonConvert.DeserializeObject<LectureModerationRequestDto>(lectureModeration.NewData);

    if (lectureModeration.LectureId == null)
    {
      var lecture = new Lecture
      {
        Id = dto.Lecture.Id,
        SectionId = dto.Lecture.SectionId,
        Title = dto.Lecture.Title,
        Description = dto.Lecture.Description,
        Order = dto.Lecture.Order,
        IsPreviewable = dto.Lecture.IsPreviewable,
        CreatedAt = DateTime.UtcNow,
      };

      _context.Lectures.Add(lecture);
      lectureModeration.LectureId = lecture.Id;
    }
    else
    {
      var lecture = await _context.Lectures.FirstOrDefaultAsync(l => l.Id == lectureModeration.LectureId);

      if (lecture == null)
      {
        return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
      }

      lecture.Title = dto.Lecture.Title;
      lecture.Description = dto.Lecture.Description;
      lecture.Order = dto.Lecture.Order;
      lecture.IsPreviewable = dto.Lecture.IsPreviewable;
    }
    lectureModeration.Status = Status.Aproved;
    lectureModeration.AdminId = adminId;
    lectureModeration.ReviewedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return ApiResponse<LectureModeration>.Success(lectureModeration, "Thao tác thành công.");
  }

  public async Task<ApiResponse<LectureModeration>> RejectLectureRequestAsync(Guid lectureRequestId, string reason, int adminId)
  {
    var lectureModeration = await _context.LectureModerations.FirstOrDefaultAsync(l => l.Id == lectureRequestId);

    if (lectureModeration == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    lectureModeration.Status = Status.Rejected;
    lectureModeration.Reason = reason;
    lectureModeration.AdminId = adminId;
    lectureModeration.ReviewedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return ApiResponse<LectureModeration>.Success(lectureModeration, "Thao tác thành công.");
  }

  public async Task<ApiResponse<List<LectureModeration>>> GetLecturesRequestAsync()
  {
    var lectureModeration = await _context.LectureModerations.ToListAsync();

    return ApiResponse<List<LectureModeration>>.Success(lectureModeration, "Lấy danh sách bài giảng.");
  }
}