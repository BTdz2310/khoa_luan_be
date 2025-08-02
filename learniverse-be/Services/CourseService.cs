using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace learniverse_be.Services;

public class CourseService : ICourseService
{
  private readonly AppDbContext _context;
  private readonly IS3Service _s3Service;
  private readonly IDtoService _dtoService;

  public CourseService(AppDbContext context, IS3Service s3Service, IDtoService dtoService)
  {
    _context = context;
    _s3Service = s3Service;
    _dtoService = dtoService;
  }

  public async Task<bool> CheckPermission(int authId, Guid courseId)
  {
    var auth = await _context.Auths
      .Include(a => a.Instructor)
      .Include(a => a.User)
      .FirstOrDefaultAsync(a => a.Id == authId);

    if (auth == null)
    {
      return false;
    }

    var course = await _context.Courses
      .FirstOrDefaultAsync(c => c.Id == courseId);

    if (course == null)
    {
      return false;
    }

    return auth.Instructor.Id == course.InstructorId;
  }

  public async Task<ApiResponse<List<CourseResponseDTO>>> GetCoursesAsync(int authId)
  {
    var auth = await _context.Auths
        .Include(a => a.Instructor)
        .FirstOrDefaultAsync(a => a.Id == authId);

    if (auth == null || auth.Instructor == null)
    {
      return ApiResponse<List<CourseResponseDTO>>.Error("Không tìm thấy tài khoản đủ điều kiện thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    var courses = await _context.Courses
        .Include(c => c.Category)
        .Where(c => c.InstructorId == auth.Instructor.Id)
        .ToListAsync();

    return ApiResponse<List<CourseResponseDTO>>.Success([.. courses.Select(c => new CourseResponseDTO
    {
      Id = c.Id,
      Category = _dtoService.CategotyToDto(c.Category),
      Instructor = _dtoService.InstructorToDto(auth.Instructor),
      Title = c.Title,
      Slug = c.Slug,
      Level = c.Level,
      ShortDescription = c.ShortDescription,
      Language = c.Language,
      Status = c.Status,
      Requirements = c.Requirements,
      LearningObjectives = c.LearningObjectives,
      Price = c.Price,
      CreatedAt = c.CreatedAt,
      UpdatedAt = c.UpdatedAt,
      Image = c.Image
    })], "Lấy danh sách khoá học.");
  }

  public async Task<bool> ValidatePermissionCourse(Course course, int identifierId, Role role)
  {
    if (role == Role.Instructor)
    {
      return course.InstructorId == identifierId;
    }
    else if (role == Role.User)
    {
      var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.UserId == identifierId && e.CourseId == course.Id);
      return enrollment != null;
    }

    return false;
  }

  public async Task<ApiResponse<List<CourseResponseDTO>>> GetAllCoursesAsync()
  {
    var courses = await _context.Courses
      .Include(c => c.Category)
      .Include(c => c.Instructor)
      .ToListAsync();

    return ApiResponse<List<CourseResponseDTO>>.Success([.. courses.Select(c => _dtoService.CourseToDto(c))], "Lấy danh sách khoá học.");
  }

  public async Task<ApiResponse<Dictionary<string, object?>>> GetCourseAsync(int? userId, string slug)
  {
    var course = await _context.Courses
      .Include(c => c.Category)
      .Include(c => c.Instructor)
      .FirstOrDefaultAsync(c => c.Slug == slug);

    if (course == null)
    {
      return ApiResponse<Dictionary<string, object?>>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    var enrollment = await _context.Enrollments
      .FirstOrDefaultAsync(e => e.UserId == userId && e.Course.Slug == slug);

    var response = new Dictionary<string, object?>
    {
      ["course"] = _dtoService.CourseToDto(course),
      ["enrollment"] = enrollment == null ? null : _dtoService.EnrollmentToDto(enrollment)
    };

    return ApiResponse<Dictionary<string, object?>>.Success(response, "Lấy thống tin khoá học.");
  }

  public async Task<ApiResponse<EnrollmentDto>> EnrollCourseAsync(int userId, string slug)
  {
    var course = await _context.Courses
      .Include(c => c.Category)
      .Include(c => c.Instructor)
      .FirstOrDefaultAsync(c => c.Slug == slug);

    if (course == null)
    {
      return ApiResponse<EnrollmentDto>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    var enrollment = await _context.Enrollments
      .FirstOrDefaultAsync(e => e.UserId == userId && e.Course.Slug == slug);

    if (enrollment != null)
    {
      return ApiResponse<EnrollmentDto>.Error("Bản khóa học đã đăng ký.", (int)HttpStatusCode.BadRequest);
    }

    var newEnrollment = new Enrollment
    {
      UserId = userId,
      CourseId = course.Id,
      EnrolledAt = DateTime.UtcNow,
      Status = EnrollmentStatus.Active,
    };

    await _context.Enrollments.AddAsync(newEnrollment);
    await _context.SaveChangesAsync();

    return ApiResponse<EnrollmentDto>.Success(_dtoService.EnrollmentToDto(newEnrollment), "Đăng ký khoá học.");
  }

  public async Task<ApiResponse<CourseResponseDTO>> GetInformationAsync(int instructorId, string slug)
  {
    var course = await _context.Courses
      .Include(c => c.Category)
      .Include(c => c.Instructor)
      .FirstOrDefaultAsync(c => c.InstructorId == instructorId && c.Slug == slug);

    if (course == null)
    {
      return ApiResponse<CourseResponseDTO>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    return ApiResponse<CourseResponseDTO>.Success(_dtoService.CourseToDto(course), "Lấy danh sách khoá học.");
  }

  public async Task<ApiResponse<CourseResponseDTO>> CreateCourseAsync(int authId, CreateCourseDto dto, IFormFile? file)
  {
    var auth = await _context.Auths
        .Include(a => a.Instructor)
        .FirstOrDefaultAsync(a => a.Id == authId);

    if (auth == null || auth.Instructor == null)
    {
      return ApiResponse<CourseResponseDTO>.Error("Không tìm thấy tài khoản đủ điều kiện thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    string? avatar = null;

    if (file != null)
    {
      using var stream = file.OpenReadStream();
      avatar = await _s3Service.UploadFileAsync(stream, $"avatars/{Guid.NewGuid()}_{file.FileName}", file.ContentType);
    }

    var course = new Course
    {
      Title = dto.Title,
      Image = avatar,
      Slug = dto.Slug,
      Level = dto.Level,
      ShortDescription = dto.ShortDescription,
      Language = dto.Language,
      Price = +dto.Price,
      Status = Status.Draft,
      Requirements = dto.Requirements,
      LearningObjectives = dto.LearningObjectives,
    };

    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
    if (category == null)
    {
      return ApiResponse<CourseResponseDTO>.Error("Không tìm thấy danh mục.", (int)HttpStatusCode.BadRequest);
    }

    if (await _context.Courses.AnyAsync(c => c.Slug == dto.Slug))
    {
      return ApiResponse<CourseResponseDTO>.Error("Slug đã tồn tại.", (int)HttpStatusCode.BadRequest);
    }

    course.Category = category;
    course.InstructorId = auth.Instructor.Id;

    _context.Courses.Add(course);
    await _context.SaveChangesAsync();

    return ApiResponse<CourseResponseDTO>.Success(new CourseResponseDTO
    {
      Id = course.Id,
      Category = _dtoService.CategotyToDto(course.Category),
      Instructor = _dtoService.InstructorToDto(auth.Instructor),
      Title = course.Title,
      Slug = course.Slug,
      ShortDescription = course.ShortDescription,
      Language = course.Language,
      Level = course.Level,
      Status = course.Status,
      Price = course.Price,
      Requirements = course.Requirements,
      LearningObjectives = course.LearningObjectives,
      CreatedAt = course.CreatedAt,
      UpdatedAt = course.UpdatedAt,
      Image = course.Image
    }, "Tạo khóa học thành công.");
  }



  // ------- SECTION ------



  public async Task<ApiResponse<SectionResponseDto>> UpdateSectionAsync(int instructorId, SectionRequestDto dto)
  {
    // Console.WriteLine($"instructorId: {instructorId}, dto.Id: {JsonSerializer.Serialize(dto)}");
    var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);

    if (course != null && course.InstructorId != instructorId)
    {
      return ApiResponse<SectionResponseDto>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
    }

    if (course == null)
    {
      return ApiResponse<SectionResponseDto>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    var existing = await _context.Sections.FirstOrDefaultAsync(s => s.Id == dto.Id);

    if (existing != null)
    {
      existing.Title = dto.Title;
      existing.Order = dto.Order;
      existing.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
    }
    else
    {
      existing = new Section
      {
        Id = dto.Id,
        Title = dto.Title,
        Order = dto.Order,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      existing.Course = course;

      _context.Sections.Add(existing);
      await _context.SaveChangesAsync();
    }

    return ApiResponse<SectionResponseDto>.Success(await _dtoService.SectionToDto(existing), "Cập nhật chủ đề thành công.");
  }

  public async Task<ApiResponse<InstructorSectionsResponseDTO>> GetSectionsAsync(string slug, int instructorId)
  {
    var course = await _context.Courses.Include(c => c.Category).Include(c => c.Instructor).AsSplitQuery()
        .AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);

    if (course != null && course.InstructorId != instructorId)
    {
      return ApiResponse<InstructorSectionsResponseDTO>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    if (course == null)
    {
      return ApiResponse<InstructorSectionsResponseDTO>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    var list = await _context.Sections
      .Include(s => s.Lectures)
      .Where(s => s.CourseId == course.Id)
      .OrderBy(s => s.Order)
      .AsNoTracking()
      .ToListAsync();

    var sectionDtos = new List<SectionResponseDto>();
    foreach (var s in list)
    {
      sectionDtos.Add(await _dtoService.SectionToDto(s));
    }

    return ApiResponse<InstructorSectionsResponseDTO>.Success(new InstructorSectionsResponseDTO
    {
      Sections = sectionDtos,
      Course = _dtoService.CourseToDto(course)
    }, "Lấy danh sách chủ đề.");
  }

  public async Task<ApiResponse<object>> DeleteSectionAsync(int instructorId, Guid sectionId)
  {
    var section = await _context.Sections.Include(c => c.Course).FirstOrDefaultAsync(s => s.Id == sectionId);

    if (section != null && section.Course.InstructorId != instructorId)
    {
      return ApiResponse<object>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
    }

    if (section == null)
    {
      return ApiResponse<object>.Error("Không tìm thấy chủ đề.", (int)HttpStatusCode.BadRequest);
    }

    _context.Sections.Remove(section);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Xóa chủ đề thành công.");
  }


  // ------- LECTURE ------



  public async Task<ApiResponse<Dictionary<string, object?>>> GetLectureAsync(int instructorId, string slug, Role role, Guid? lectureId)
  {
    Console.WriteLine("--------->GetLectureAsync");
    Lecture? lecture;
    if (lectureId.HasValue)
    {
      lecture = await _context.Lectures
        .Include(l => l.Section)
          .ThenInclude(s => s.Course)
          .ThenInclude(c => c.Sections)
          .ThenInclude(s => s.Lectures)
        .Include(l => l.Section)
          .ThenInclude(s => s.Course)
          .ThenInclude(c => c.Category)
        .Include(l => l.Section)
          .ThenInclude(s => s.Course)
          .ThenInclude(c => c.Instructor)
        .Include(l => l.Video)
        .FirstOrDefaultAsync(l => l.Id == lectureId);

      // if (lecture != null && lecture.Section.Course.InstructorId != instructorId)
      // {
      //   return ApiResponse<Dictionary<string, object?>>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
      // }

      if (lecture == null)
      {
        return ApiResponse<Dictionary<string, object?>>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
      }
    }
    else
    {
      lecture = await _context.Lectures
        .Include(l => l.Section)
          .ThenInclude(s => s.Course)
          .ThenInclude(c => c.Sections)
          .ThenInclude(s => s.Lectures)
        .Include(l => l.Section)
          .ThenInclude(s => s.Course)
          .ThenInclude(c => c.Category)
        .Include(l => l.Section)
          .ThenInclude(s => s.Course)
          .ThenInclude(c => c.Instructor)
        .Where(l => l.Section.Course.Slug == slug)
        .OrderBy(l => l.Section.Order).ThenBy(l => l.Order)
        .FirstOrDefaultAsync();
    }

    if (lecture == null)
    {
      return ApiResponse<Dictionary<string, object?>>.Success(null);
    }

    var signedUrl = (lecture.Video == null || lecture.Video.HlsDirectoryKey == null) ? null : _s3Service.GenerateSignedUrl(lecture.Video.HlsDirectoryKey);

    var chatsDto = new List<VideoChatResponseDto>();
    if (lecture.Video != null)
    {
      var chats = await _context.VideoChats.Include(c => c.Video).ThenInclude(c => c.Lecture).ThenInclude(l => l.Section).ThenInclude(s => s.Course).Include(c => c.User).Include(c => c.Instructor).Where(c => c.VideoId == lecture.Video.Id).ToListAsync();
      chatsDto = chats.Select(c => _dtoService.VideoChatToDto(c)).ToList();
    }

    var response = new Dictionary<string, object?>
    {
      ["lecture"] = new LectureResponseDTO
      {
        Id = lecture.Id,
        Title = lecture.Title,
        Description = lecture.Description,
        Order = lecture.Order,
        SectionId = lecture.SectionId,
        IsPreviewable = lecture.IsPreviewable,
        CreatedAt = lecture.CreatedAt,
        UpdatedAt = lecture.UpdatedAt,
      },
      ["structure"] = lecture.Section.Course.Sections.Select(s => _dtoService.StructureCourseDto(s)).ToList(),
      ["course"] = _dtoService.CourseToDto(lecture.Section.Course),
      ["video"] = lecture.Video == null ? null : _dtoService.VideoToDto(lecture.Video),
      ["signedUrl"] = signedUrl,
      ["chats"] = chatsDto
    };

    return ApiResponse<Dictionary<string, object?>>.Success(response);
  }

  public async Task<ApiResponse<LectureModeration>> UpdateLectureRequestAsync(int instructorId, LectureRequestDto dto, Guid id)
  {
    Console.WriteLine("--------->UpdateLectureRequestAsync" + dto.SectionId + dto.Id + dto.Title + dto.Description + dto.Order + dto.IsPreviewable);
    var course = await _context.Sections.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == dto.SectionId);

    if (course != null && course.Course.InstructorId != instructorId)
    {
      return ApiResponse<LectureModeration>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    if (course == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    var lecture = new LectureModerationRequestDto
    {
      Lecture = dto,
      Video = null
    };

    var lectureModeration = await _context.LectureModerations.FirstOrDefaultAsync(l => l.Id == id);

    if (lectureModeration == null)
    {
      lectureModeration = new LectureModeration
      {
        Id = id,
        ActionType = ModerationAction.Update,
        NewData = JsonSerializer.Serialize(lecture),
        Status = Status.Pending,
        Reason = "",
        InstructorId = instructorId,
        SectionId = dto.SectionId,
      };
      _context.LectureModerations.Add(lectureModeration);
    }
    else
    {
      lectureModeration.ActionType = ModerationAction.Update;
      lectureModeration.Status = Status.Pending;
      lectureModeration.NewData = JsonSerializer.Serialize(lecture);
      lectureModeration.AdminId = null;
    }

    await _context.SaveChangesAsync();

    return ApiResponse<LectureModeration>.Success(lectureModeration, "Thao tác thành công, vui lòng chờ duyệt từ Admin.");
  }

  public async Task<ApiResponse<LectureModeration>> CancelLectureRequestAsync(int instructorId, Guid sectionId, Guid id)
  {
    var course = await _context.Sections.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == sectionId);

    if (course != null && course.Course.InstructorId != instructorId)
    {
      return ApiResponse<LectureModeration>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    if (course == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    var lectureModeration = await _context.LectureModerations.FirstOrDefaultAsync(l => l.Id == id);

    if (lectureModeration == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }
    else
    {
      lectureModeration.Status = Status.Canceled;
    }

    await _context.SaveChangesAsync();

    return ApiResponse<LectureModeration>.Success(lectureModeration, "Thao tác thành công, vui lòng chờ duyệt từ Admin.");
  }

  public async Task<ApiResponse<LectureModeration>> DeleteLectureRequestAsync(int instructorId, Guid id)
  {
    var lectureModeration = _context.LectureModerations.FirstOrDefault(l => l.Id == id);

    var course = await _context.Sections.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == lectureModeration.SectionId);

    if (course != null && course.Course.InstructorId != instructorId)
    {
      return ApiResponse<LectureModeration>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    if (course == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    if (lectureModeration == null)
    {
      return ApiResponse<LectureModeration>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    if (lectureModeration.LectureId == null)
    {
      _context.LectureModerations.Remove(lectureModeration);
      return ApiResponse<LectureModeration>.Success(null, "Thao tác thành công.");
    }
    else
    {
      lectureModeration.ActionType = ModerationAction.Delete;
      lectureModeration.Status = Status.Pending;
      lectureModeration.NewData = "";
      lectureModeration.AdminId = null;
      _context.LectureModerations.Update(lectureModeration);
    }

    await _context.SaveChangesAsync();

    return ApiResponse<LectureModeration>.Success(lectureModeration, "Thao tác thành công, vui lòng chờ duyệt từ Admin.");
  }

  public async Task<ApiResponse<Dictionary<string, object?>>> GenerateUploadUrlAsync(int instructorId, Guid lectureId, UploadRequestDto dto)
  {
    var lecture = await _context.Lectures.Include(l => l.Section).ThenInclude(s => s.Course).Include(l => l.Video).FirstOrDefaultAsync(l => l.Id == lectureId);

    if (lecture != null && lecture.Section.Course.InstructorId != instructorId)
    {
      return ApiResponse<Dictionary<string, object?>>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    if (lecture == null)
    {
      return ApiResponse<Dictionary<string, object?>>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    if (lecture.Video != null)
    {
      if (lecture.Video.Status == VideoProcessingStatus.Failed)
      {
        _context.Videos.Remove(lecture.Video);
      }
      else
      {
        return ApiResponse<Dictionary<string, object?>>.Error("Bài giảng đã tạo video.", (int)HttpStatusCode.BadRequest);
      }
    }

    var allowedTypes = new[] { "video/mp4", "video/quicktime" };
    if (!allowedTypes.Contains(dto.OriginalMimeType))
    {
      return ApiResponse<Dictionary<string, object?>>.Error("Loại file không hợp lệ.", (int)HttpStatusCode.BadRequest);
    }

    var Id = Guid.NewGuid();
    var extension = dto.OriginalMimeType == "video/mp4" ? "mp4" : "mov";
    var filePath = $"videos/{lectureId}/{Id}.{extension}";

    var video = new Video
    {
      Id = Id,
      OriginalFileName = dto.OriginalFileName,
      OriginalMimeType = dto.OriginalMimeType,
      OriginalFileKey = filePath,
      FileSize = dto.FileSize,
      LectureId = lectureId,
      Status = VideoProcessingStatus.Pending,
      UploadedAt = DateTime.UtcNow,
      Width = dto.Width,
      Height = dto.Height,
      DurationSeconds = dto.DurationSeconds
    };

    _context.Videos.Add(video);
    await _context.SaveChangesAsync();

    var url = _s3Service.GenerateUploadUrl(filePath, TimeSpan.FromMinutes(15), dto.OriginalMimeType);

    var response = new Dictionary<string, object?>
    {
      ["video"] = _dtoService.VideoToDto(video),
      ["url"] = url
    };

    return ApiResponse<Dictionary<string, object?>>.Success(response);
  }

  public async Task<ApiResponse<object>> UpdateVideoHlsAsync(string key, UploadHlsVideoDto dto)
  {
    var video = await _context.Videos.FirstOrDefaultAsync(v => v.OriginalFileKey == key);

    if (video == null)
    {
      return ApiResponse<object>.Error("Không tìm thấy video.", (int)HttpStatusCode.BadRequest);
    }

    video.HlsMasterPlaylistUrl = dto.HlsMasterPlaylistUrl;
    video.HlsDirectoryKey = dto.HlsDirectoryKey;
    video.Status = VideoProcessingStatus.Completed;
    video.UploadedAt = DateTime.UtcNow;

    _context.Videos.Update(video);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Cập nhật video thành công.");
  }

  public async Task<ApiResponse<object>> FailedVideoHlsAsync(string key)
  {
    var video = await _context.Videos.FirstOrDefaultAsync(v => v.OriginalFileKey == key);

    if (video == null)
    {
      return ApiResponse<object>.Error("Không tìm thấy video.", (int)HttpStatusCode.BadRequest);
    }

    video.Status = VideoProcessingStatus.Failed;
    video.UploadedAt = DateTime.UtcNow;

    _context.Videos.Update(video);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Cập nhật video thành công.");
  }
}