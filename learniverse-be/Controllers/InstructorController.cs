using System.Net;
using System.Security.Claims;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/instructor")]
public class InstructorController(ILogger<InstructorController> logger, IInstructorService instructorService) : ControllerBase
{
  private readonly ILogger<InstructorController> _logger = logger;
  private readonly IInstructorService _instructorService = instructorService;

  [Authorize]
  [HttpPost("")]
  public async Task<ActionResult<ApiResponse<InstructorResponseDto>>> CreateInstructor([FromForm] CreateInstructorDto dto, IFormFile? file)
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return ApiResponse<InstructorResponseDto>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized);
    }

    var result = await _instructorService.CreateInstructorAsync(authIdInt, dto, file);
    return StatusCode(result.StatusCode, result);
  }
}