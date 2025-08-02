using learniverse_be.DTOs;
using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface ICourseService
{
  public Task<ApiResponse<CourseResponseDTO>> CreateCourseAsync(int authId, CreateCourseDto dto, IFormFile? file);
  public Task<ApiResponse<List<CourseResponseDTO>>> GetCoursesAsync(int authId);
  public Task<ApiResponse<Dictionary<string, object?>>> GetCourseAsync(int? userId, string slug);
  public Task<ApiResponse<EnrollmentDto>> EnrollCourseAsync(int userId, string slug);
  public Task<ApiResponse<List<CourseResponseDTO>>> GetAllCoursesAsync();
  public Task<ApiResponse<CourseResponseDTO>> GetInformationAsync(int instructorId, string slug);
  public Task<ApiResponse<InstructorSectionsResponseDTO>> GetSectionsAsync(string courseId, int instructorId);
  public Task<ApiResponse<SectionResponseDto>> UpdateSectionAsync(int instructorId, SectionRequestDto section);
  public Task<ApiResponse<object>> DeleteSectionAsync(int instructorId, Guid sectionId);
  public Task<ApiResponse<LectureModeration>> UpdateLectureRequestAsync(int instructorId, LectureRequestDto lecture, Guid id);
  public Task<ApiResponse<LectureModeration>> DeleteLectureRequestAsync(int instructorId, Guid id);
  public Task<ApiResponse<LectureModeration>> CancelLectureRequestAsync(int instructorId, Guid sectionId, Guid id);
  public Task<ApiResponse<Dictionary<string, object?>>> GetLectureAsync(int identifierId, string slug, Role role, Guid? lectureId);
  public Task<ApiResponse<Dictionary<string, object?>>> GenerateUploadUrlAsync(int instructorId, Guid lectureId, UploadRequestDto dto);
  public Task<ApiResponse<object>> UpdateVideoHlsAsync(string key, UploadHlsVideoDto dto);
  public Task<ApiResponse<object>> FailedVideoHlsAsync(string key);
  public Task<bool> ValidatePermissionCourse(Course course, int identifierId, Role role);
}