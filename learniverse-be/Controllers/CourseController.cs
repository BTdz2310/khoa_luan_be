using System.Net;
using System.Security.Claims;
using System.Text.Json;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/course")]
public class CourseController(ILogger<CourseController> logger, ICourseService courseService, IConfiguration config) : ControllerBase
{
  private readonly ILogger<CourseController> _logger = logger;
  private readonly ICourseService _courseService = courseService;
  private readonly IConfiguration _config = config;

  [Authorize]
  [HttpPost("instructor")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> CreateCourse([FromForm] CreateCourseDto dto, IFormFile? file)
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.CreateCourseAsync(authIdInt, dto, file);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> GetAllCourses()
  {
    var result = await _courseService.GetAllCoursesAsync();
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("{slug}")]
  [Authorize]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> GetCourse(string slug)
  {
    var userId = User.FindFirstValue("userId");

    var result = await _courseService.GetCourseAsync(userId != null ? int.Parse(userId) : (int?)null, slug);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpGet("instructor")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> GetCourses()
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.GetCoursesAsync(authIdInt);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpPost("{slug}/enroll")]
  public async Task<ActionResult<ApiResponse<EnrollmentDto>>> EnrollCourse([FromRoute] string slug)
  {
    var iId = User.FindFirstValue("userId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.EnrollCourseAsync(iIdInt, slug);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpGet("instructor/{slug}")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> GetInformation([FromRoute] string slug)
  {
    var iId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.GetInformationAsync(iIdInt, slug);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpPost("instructor/section")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> UpdateSection([FromBody] SectionRequestDto dto)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.UpdateSectionAsync(instructorIdInt, dto);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpDelete("instructor/section/{id}")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> DeleteSection([FromRoute] Guid id)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.DeleteSectionAsync(instructorIdInt, id);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpGet("instructor/section/{slug}")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> GetSections([FromRoute] string slug)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.GetSectionsAsync(slug, instructorIdInt);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpPost("instructor/lecture/{id}")]
  public async Task<ActionResult<ApiResponse<Lecture>>> UpdateLectureRequest([FromBody] LectureRequestDto dto, [FromRoute] Guid id)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<Lecture>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.UpdateLectureRequestAsync(instructorIdInt, dto, id);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpDelete("instructor/lecture/{id}")]
  public async Task<ActionResult<ApiResponse<Lecture>>> DeleteLectureRequest([FromRoute] Guid id)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<Lecture>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.DeleteLectureRequestAsync(instructorIdInt, id);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpDelete("instructor/lecture/{sectionId}/{id}/cancel")]
  public async Task<ActionResult<ApiResponse<Lecture>>> CancelLectureRequest([FromRoute] Guid id, [FromRoute] Guid sectionId)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<Lecture>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.CancelLectureRequestAsync(instructorIdInt, sectionId, id);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("instructor/lecture/{slug}")]
  public async Task<ActionResult<object>> GetLecture([FromRoute] string slug, [FromQuery] Guid? lectureId, [FromQuery] Role role)
  {
    // var instructorId = User.FindFirstValue("instructorId");
    var identifierId = role == Role.User ? User.FindFirstValue("userId") : User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(identifierId) || !int.TryParse(identifierId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<Lecture>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.GetLectureAsync(instructorIdInt, slug, role, lectureId);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPost("instructor/lecture/{lectureId}/upload-url")]
  public async Task<ActionResult<Dictionary<string, object?>>> GenerateUploadUrl(Guid lectureId, [FromBody] UploadRequestDto dto)
  {
    var instructorId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(instructorId) || !int.TryParse(instructorId, out var instructorIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<Lecture>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.GenerateUploadUrlAsync(instructorIdInt, lectureId, dto);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPut("instructor/video-hls/{key}")]
  public async Task<ActionResult<object>> UpdateVideoHls([FromRoute] string key, [FromBody] UploadHlsVideoDto dto)
  {
    var secretFromHeader = Request.Headers["X-Api-Secret"].FirstOrDefault();
    var expectedSecret = _config["Security:BackendApiSecret"];
    var decodedKey = Uri.UnescapeDataString(key);

    if (secretFromHeader != expectedSecret)
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<object>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.UpdateVideoHlsAsync(decodedKey, dto);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPut("instructor/video-hls/{key}/failure")]
  public async Task<ActionResult<object>> FailedVideoHls([FromRoute] string key)
  {
    var secretFromHeader = Request.Headers["X-Api-Secret"].FirstOrDefault();
    var expectedSecret = _config["Security:BackendApiSecret"];
    var decodedKey = Uri.UnescapeDataString(key);

    if (secretFromHeader != expectedSecret)
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<object>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _courseService.FailedVideoHlsAsync(decodedKey);
    return StatusCode(result.StatusCode, result);
  }
}