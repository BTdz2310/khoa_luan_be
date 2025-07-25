using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface ICourseService
{
  public Task<ApiResponse<CourseResponseDTO>> CreateCourseAsync(int authId, CreateCourseDto dto, IFormFile? file);
  public Task<ApiResponse<List<CourseResponseDTO>>> GetCoursesAsync(int authId);
  public Task<ApiResponse<CourseResponseDTO>> GetInformationAsync(int instructorId, string slug);
  public CourseResponseDTO CourseToDto(Course course);
  public Task<ApiResponse<InstructorSectionsResponseDTO>> GetSectionsAsync(string courseId, int instructorId);
  public Task<ApiResponse<Section>> UpdateSectionAsync(int instructorId, SectionRequestDto section);
  public Task<ApiResponse<object>> DeleteSectionAsync(int instructorId, Guid sectionId);
  public Task<ApiResponse<LectureResponseDTO>> UpdateLectureAsync(int instructorId, LectureRequestDto lecture);
  public Task<ApiResponse<object>> DeleteLectureAsync(int instructorId, Guid lectureId);
  public Task<ApiResponse<Dictionary<string, object?>>> GetLectureAsync(int instructorId, string slug, Guid? lectureId);
  public Task<ApiResponse<Dictionary<string, object?>>> GenerateUploadUrlAsync(int instructorId, Guid lectureId, UploadRequestDto dto);
  public Task<ApiResponse<object>> UpdateVideoHlsAsync(string key, UploadHlsVideoDto dto);
  public Task<ApiResponse<object>> FailedVideoHlsAsync(string key);
}