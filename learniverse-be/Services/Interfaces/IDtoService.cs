using learniverse_be.DTOs;
using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IDtoService
{
  public CategoriesDto CategotyToDto(Category category);
  public UserDto UserToDto(User user);
  public EnrollmentDto EnrollmentToDto(Enrollment enrollment);
  public InstructorResponseDto InstructorToDto(Instructor instructor);
  public VideoChatResponseDto VideoChatToDto(VideoChat chat);
  public CourseResponseDTO CourseToDto(Course course);
  public LectureResponseDTO LectureToDto(Lecture lecture);
  public SectionSDto StructureCourseDto(Section section);
  public Task<SectionResponseDto> SectionToDto(Section lecture);
  public VideoResponseDto VideoToDto(Video video);
  public LivestreamResponseDto LivestreamToDto(Livestream livestream);
}