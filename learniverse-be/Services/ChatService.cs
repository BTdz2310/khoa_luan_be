using System.Net;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace learniverse_be.Services;

public class ChatService : IChatService
{
  private readonly AppDbContext _context;
  private readonly IDtoService _dtoService;
  public readonly ICourseService _courseService;

  public ChatService(AppDbContext context, IDtoService dtoService, ICourseService courseService)
  {
    _context = context;
    _dtoService = dtoService;
    _courseService = courseService;
  }

  public async Task<ApiResponse<VideoChatResponseDto>> CreateChatVideo(int identifierId, VideoChatRequestDto dto, Role role)
  {
    Console.WriteLine("--------->CreateChatVideo");
    var course = dto.VideoId != null ? (await _context.Videos.Include(v => v.Lecture).ThenInclude(l => l.Section).ThenInclude(s => s.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(v => v.Id == dto.VideoId)).Lecture.Section.Course : (await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.Id == dto.LivestreamId)).Course;
    var isValid = await _courseService.ValidatePermissionCourse(course, identifierId, role);

    Console.WriteLine($"------> IS VALID: {isValid}");

    if (!isValid)
    {
      return ApiResponse<VideoChatResponseDto>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    if (dto.VideoId != null)
    {
      var lecture = await _context.Videos.FirstOrDefaultAsync(l => l.Id == dto.VideoId);
      if (lecture == null)
      {
        return ApiResponse<VideoChatResponseDto>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
      }
    }

    if (dto.LivestreamId != null)
    {
      var lecture = await _context.Livestreams.FirstOrDefaultAsync(l => l.Id == dto.LivestreamId);
      if (lecture == null)
      {
        return ApiResponse<VideoChatResponseDto>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
      }
    }

    var chat = new VideoChat
    {
      Content = dto.Content,
      VideoTimestamp = dto.VideoTimestamp,
      CreatedAt = DateTime.UtcNow,
      ParentId = dto.ParentId
    };

    _ = dto.VideoId != null ? chat.VideoId = dto.VideoId : chat.LivestreamId = dto.LivestreamId;
    _ = role == Role.User ? chat.UserId = identifierId : chat.InstructorId = identifierId;

    await _context.VideoChats.AddAsync(chat);
    await _context.SaveChangesAsync();

    await _context.Entry(chat)
      .Reference(c => c.Video)
      .Query()
      .Include(v => v.Lecture)
      .ThenInclude(l => l.Section)
      .LoadAsync();

    await _context.Entry(chat)
      .Reference(c => c.User)
      .Query()
      .LoadAsync();

    await _context.Entry(chat)
      .Reference(c => c.Instructor)
      .Query()
      .LoadAsync();

    return ApiResponse<VideoChatResponseDto>.Success(_dtoService.VideoChatToDto(chat), "Tạo chat thành cong.");
  }
}