using System.Net;
using System.Security.Claims;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/live")]
public class LiveController(ILiveService liveService) : ControllerBase
{
  private readonly ILiveService _liveService = liveService;

  [HttpGet("range/{slug}/{month}/{year}")]
  public async Task<ActionResult<ApiResponse<List<LivestreamResponseDto>>>> GetLivesByMonth([FromRoute] string slug, [FromRoute] int month, [FromRoute] int year)
  {
    var iId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _liveService.GetLivesByMonthAsync(iIdInt, slug, month, year);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPost("range/{courseId}")]
  public async Task<ActionResult<ApiResponse<LivestreamResponseDto>>> CreateLive([FromRoute] Guid courseId, [FromBody] LivestreamRequestDto dto)
  {
    var iId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _liveService.CreateLiveAsync(iIdInt, courseId, dto);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPut("range/{id}")]
  public async Task<ActionResult<ApiResponse<LivestreamResponseDto>>> UpdateLive([FromBody] LivestreamRequestDto dto, [FromRoute] Guid id)
  {
    var iId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _liveService.UpdateLiveAsync(iIdInt, dto, id);
    return StatusCode(result.StatusCode, result);
  }

  [HttpDelete("range/{id}")]
  public async Task<ActionResult<ApiResponse<object>>> DeleteLive([FromRoute] Guid id)
  {
    var iId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _liveService.DeleteLiveAsync(iIdInt, id);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPost("active/{id}")]
  public async Task<ActionResult<ApiResponse<object>>> ActiveLive([FromRoute] Guid id)
  {
    var iId = User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iId) || !int.TryParse(iId, out var iIdInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _liveService.ActiveLiveAsync(iIdInt, id);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("livestream/{id}")]
  public async Task<ActionResult<ApiResponse<object>>> GetLive([FromRoute] Guid id, [FromQuery] Role role)
  {
    var iIdentifier = role == Role.User ? User.FindFirstValue("userId") : User.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(iIdentifier) || !int.TryParse(iIdentifier, out var iIdentifierInt))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _liveService.GetLiveAsync(id, iIdentifierInt, role);
    return StatusCode(result.StatusCode, result);
  }

  [HttpPost("start")]
  public async Task<IActionResult> StartStream([FromForm] StreamRequest request)
  {
    var streamKey = request.Name;

    if (!await _liveService.ValidateStreamKey(streamKey))
      return BadRequest();

    await _liveService.StartProcessing(request);

    return Ok();
  }

  [HttpPost("end")]
  public async Task<IActionResult> EndStream([FromForm] StreamRequest request)
  {
    await _liveService.StopProcessing(request);
    return Ok();
  }
}

public class StreamRequest
{
    public Guid Name { get; set; }
    public string Addr { get; set; }
}