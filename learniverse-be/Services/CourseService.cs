using System.Net;
using System.Reflection.PortableExecutable;
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
  private readonly IInstructorService _instructorService;
  private readonly ICategoryService _categoryService;

  public CourseService(AppDbContext context, IS3Service s3Service, IInstructorService instructorService, ICategoryService categoryService)
  {
    _context = context;
    _s3Service = s3Service;
    _instructorService = instructorService;
    _categoryService = categoryService;
  }

  public CourseResponseDTO CourseToDto(Course course)
  {
    return new CourseResponseDTO
    {
      Id = course.Id,
      Category = _categoryService.CategotyToDto(course.Category),
      Instructor = _instructorService.InstructorToDto(course.Instructor),
      Title = course.Title,
      Slug = course.Slug,
      Level = course.Level,
      ShortDescription = course.ShortDescription,
      Language = course.Language,
      Status = course.Status,
      Requirements = course.Requirements,
      LearningObjectives = course.LearningObjectives,
      Price = course.Price,
      CreatedAt = course.CreatedAt,
      UpdatedAt = course.UpdatedAt,
      Image = course.Image
    };
  }

  public LectureResponseDTO LectureToDto(Lecture lecture)
  {
    return new LectureResponseDTO
    {
      Id = lecture.Id,
      Title = lecture.Title,
      Description = lecture.Description,
      Order = lecture.Order,
      SectionId = lecture.SectionId,
      IsPreviewable = lecture.IsPreviewable,
      CreatedAt = lecture.CreatedAt,
      UpdatedAt = lecture.UpdatedAt,
      Status = lecture.Status,
    };
  }

  public SectionSDto StructureCourse(Section section)
  {
    return new SectionSDto
    {
      Id = section.Id,
      Title = section.Title,
      Order = section.Order,
      CourseId = section.CourseId,
      Lectures = [.. section.Lectures.Select(l => new LectureSDto {
        Id = l.Id,
        Title = l.Title,
        Order = l.Order,
        SectionId = l.SectionId,
        IsPreviewable = l.IsPreviewable,
        Status = l.Status
      })]
    };
  }

  public SectionResponseDto SectionToDto(Section lecture)
  {
    return new SectionResponseDto
    {
      Id = lecture.Id,
      Title = lecture.Title,
      CourseId = lecture.CourseId,
      Order = lecture.Order,
      CreatedAt = lecture.CreatedAt,
      UpdatedAt = lecture.UpdatedAt,
      Lectures = [.. lecture.Lectures.Select(l => LectureToDto(l))]
    };
  }

  public VideoResponseDto VideoToDto(Video video)
  {
    return new VideoResponseDto
    {
      Id = video.Id,
      LectureId = video.LectureId,
      LivestreamId = video.LivestreamId,
      OriginalFileName = video.OriginalFileName,
      OriginalFileKey = video.OriginalFileKey,
      OriginalMimeType = video.OriginalMimeType,
      FileSize = video.FileSize,
      HlsMasterPlaylistUrl = video.HlsMasterPlaylistUrl,
      HlsDirectoryKey = video.HlsDirectoryKey,
      DurationSeconds = video.DurationSeconds,
      Width = video.Width,
      Height = video.Height,
      Status = video.Status,
      UploadedAt = video.UploadedAt,
    };
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
      Category = _categoryService.CategotyToDto(c.Category),
      Instructor = _instructorService.InstructorToDto(auth.Instructor),
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

    return ApiResponse<CourseResponseDTO>.Success(CourseToDto(course), "Lấy danh sách khoá học.");
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
      Category = _categoryService.CategotyToDto(course.Category),
      Instructor = _instructorService.InstructorToDto(auth.Instructor),
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

  public async Task<ApiResponse<Section>> UpdateSectionAsync(int instructorId, SectionRequestDto dto)
  {
    // Console.WriteLine($"instructorId: {instructorId}, dto.Id: {JsonSerializer.Serialize(dto)}");
    var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);

    if (course != null && course.InstructorId != instructorId)
    {
      return ApiResponse<Section>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
    }

    if (course == null)
    {
      return ApiResponse<Section>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
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

    return ApiResponse<Section>.Success(existing, "Cập nhật chủ đề thành công.");
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

    var sectionDtos = list.Select(s => SectionToDto(s)).ToList();

    return ApiResponse<InstructorSectionsResponseDTO>.Success(new InstructorSectionsResponseDTO
    {
      Sections = sectionDtos,
      Course = CourseToDto(course)
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

  public async Task<ApiResponse<LectureResponseDTO>> UpdateLectureAsync(int instructorId, LectureRequestDto dto)
  {
    var section = await _context.Sections.Include(c => c.Course).FirstOrDefaultAsync(s => s.Id == dto.SectionId);

    if (section != null && section.Course.InstructorId != instructorId)
    {
      return ApiResponse<LectureResponseDTO>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
    }

    if (section == null)
    {
      return ApiResponse<LectureResponseDTO>.Error("Không tìm thấy chủ đề.", (int)HttpStatusCode.BadRequest);
    }

    var existing = await _context.Lectures.FirstOrDefaultAsync(l => l.Id == dto.Id);
    if (existing != null)
    {
      existing.Title = dto.Title;
      existing.Order = dto.Order;
      existing.UpdatedAt = DateTime.UtcNow;
      existing.Description = dto.Description;
      existing.IsPreviewable = dto.IsPreviewable;

      await _context.SaveChangesAsync();
    }
    else
    {
      existing = new Lecture
      {
        Id = dto.Id,
        Title = dto.Title,
        Order = dto.Order,
        Description = dto.Description,
        IsPreviewable = dto.IsPreviewable,
        Status = Status.Pending,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      existing.Section = section;

      _context.Lectures.Add(existing);
      await _context.SaveChangesAsync();
    }

    return ApiResponse<LectureResponseDTO>.Success(LectureToDto(existing), "Cập nhật bài giảng thành công.");
  }

  public async Task<ApiResponse<object>> DeleteLectureAsync(int instructorId, Guid lectureId)
  {
    var lecture = await _context.Lectures.Include(l => l.Section).ThenInclude(s => s.Course).FirstOrDefaultAsync(l => l.Id == lectureId);

    if (lecture != null && lecture.Section.Course.InstructorId != instructorId)
    {
      return ApiResponse<object>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
    }

    if (lecture == null)
    {
      return ApiResponse<object>.Error("Không tìm thấy bài giảng.", (int)HttpStatusCode.BadRequest);
    }

    _context.Lectures.Remove(lecture);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Xóa bài giảng thành công.");
  }

  public async Task<ApiResponse<Dictionary<string, object?>>> GetLectureAsync(int instructorId, string slug, Guid? lectureId)
  {
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
        .FirstOrDefaultAsync(l => l.Id == lectureId);

      if (lecture != null && lecture.Section.Course.InstructorId != instructorId)
      {
        return ApiResponse<Dictionary<string, object?>>.Error("Không đủ quyền thực hiện hành động này.", (int)HttpStatusCode.Unauthorized);
      }

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
        Status = lecture.Status
      },
      ["structure"] = lecture.Section.Course.Sections.Select(s => StructureCourse(s)).ToList(),
      ["course"] = CourseToDto(lecture.Section.Course)
    };

    return ApiResponse<Dictionary<string, object?>>.Success(response);
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
      UploadedAt = DateTime.UtcNow
    };

    _context.Videos.Add(video);
    await _context.SaveChangesAsync();

    var url = _s3Service.GenerateUploadUrl(filePath, TimeSpan.FromMinutes(15), dto.OriginalMimeType);

    var response = new Dictionary<string, object?>
    {
      ["video"] = VideoToDto(video),
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
    video.DurationSeconds = dto.DurationSeconds;
    video.Width = dto.Width;
    video.Height = dto.Height;
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