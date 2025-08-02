using System.Net;
using System.Security.Claims;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/admin")]
public class AdminController(ILogger<AdminController> logger, IAdminService adminService) : ControllerBase
{
  private readonly ILogger<AdminController> _logger = logger;
  private readonly IAdminService _adminService = adminService;

  [Authorize]
  [HttpPost("lecture-request/{id}/accept")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> AcceptLectureRequest([FromRoute] Guid id)
  {
    var userId = User.FindFirstValue("userId");
    var role = User.FindFirstValue("role");

    if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt) || role != Role.Admin.ToString())
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _adminService.AcceptLectureRequestAsync(id, userIdInt);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpPost("lecture-request/{id}/reject")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> RejectLectureRequest([FromRoute] Guid id, [FromBody] string reason)
  {
    var userId = User.FindFirstValue("userId");
    var role = User.FindFirstValue("role");

    if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt) || role != Role.Admin.ToString())
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _adminService.RejectLectureRequestAsync(id, reason, userIdInt);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [HttpGet("lecture-request")]
  public async Task<ActionResult<ApiResponse<CourseResponseDTO>>> GetLecturesRequest()
  {
    var userId = User.FindFirstValue("userId");
    Console.WriteLine(User.IsInRole("Admin"));

    if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt) || !User.IsInRole("Admin"))
    {
      return StatusCode((int)HttpStatusCode.Unauthorized, ApiResponse<CourseResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized));
    }

    var result = await _adminService.GetLecturesRequestAsync();
    return StatusCode(result.StatusCode, result);
  }
}