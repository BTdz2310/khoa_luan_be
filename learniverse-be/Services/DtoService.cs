using System.Net;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace learniverse_be.Services;

public class DtoService : IDtoService
{

  private readonly AppDbContext _context;

  public DtoService(AppDbContext context)
  {
    _context = context;
  }

  public UserDto UserToDto(User user)
  {
    return new UserDto
    {
      UserId = user.Id,
      FullName = user.FullName,
      Avatar = user.Avatar,
      Bio = user.Bio,
      BirthDate = user.BirthDate,
      Gender = user.Gender,
      Interestings = new List<int>()
    };
  }

  public InstructorResponseDto InstructorToDto(Instructor instructor)
  {
    return new InstructorResponseDto
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
    };
  }

  public VideoChatResponseDto VideoChatToDto(VideoChat chat)
  {
    return new VideoChatResponseDto
    {
      Id = chat.Id,
      VideoId = chat.VideoId,
      LivestreamId = chat.LivestreamId,
      Content = chat.Content,
      VideoTimestamp = chat.VideoTimestamp,
      CreatedAt = chat.CreatedAt,
      ParentId = chat.ParentId,
      User = chat.User != null ? UserToDto(chat.User) : null,
      Instructor = chat.Instructor != null ? InstructorToDto(chat.Instructor) : null
    };
  }

  public CategoriesDto CategotyToDto(Category category)
  {
    return new CategoriesDto
    {
      Id = category.Id,
      Code = category.Code,
      Name = category.Name,
      Description = category.Description,
      IconUrl = category.IconUrl,
      Color = category.Color
    };
  }

  public CourseResponseDTO CourseToDto(Course course)
  {
    return new CourseResponseDTO
    {
      Id = course.Id,
      Category = CategotyToDto(course.Category),
      Instructor = InstructorToDto(course.Instructor),
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

  public EnrollmentDto EnrollmentToDto(Enrollment enrollment)
  {
    return new EnrollmentDto
    {
      Id = enrollment.Id,
      UserId = enrollment.UserId,
      CourseId = enrollment.CourseId,
      Status = enrollment.Status,
      EnrolledAt = enrollment.EnrolledAt,
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
    };
  }

  public SectionSDto StructureCourseDto(Section section)
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
      })]
    };
  }

  public async Task<SectionResponseDto> SectionToDto(Section lecture)
  {
    var tempLectures = await _context.LectureModerations.Where(l => l.SectionId == lecture.Id).ToListAsync();
    return new SectionResponseDto
    {
      Id = lecture.Id,
      Title = lecture.Title,
      CourseId = lecture.CourseId,
      Order = lecture.Order,
      CreatedAt = lecture.CreatedAt,
      UpdatedAt = lecture.UpdatedAt,
      Lectures = [.. lecture.Lectures.Select(l => LectureToDto(l))],
      LectureModerations = tempLectures
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
  
  public LivestreamResponseDto LivestreamToDto(Livestream livestream)
  {
    return new LivestreamResponseDto
    {
      Id = livestream.Id,
      Course = CourseToDto(livestream.Course),
      StreamKey = livestream.StreamKey,
      StartTime = livestream.StartTime,
      EndTime = livestream.EndTime,
      Status = livestream.Status,
      Title = livestream.Title,
      PlaybackUrl = livestream.PlaybackUrl,
      CreatedAt = livestream.CreatedAt,
      UpdatedAt = livestream.UpdatedAt,
    };
  }
}