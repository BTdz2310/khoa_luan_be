using System.Net;
using System.Security.Claims;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/account")]
public class AccountController(ILogger<AccountController> logger, IAccountService accountService) : ControllerBase
{
  private readonly ILogger<AccountController> _logger = logger;
  private readonly IAccountService _accountService = accountService;

  [Authorize]
  [HttpGet("profile")]
  public async Task<ActionResult<ApiResponse<UserResponseDTO>>> GetProfile()
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return ApiResponse<UserResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized);
    }

    var result = await _accountService.GetProfileAsync(authIdInt);
    return StatusCode(result.StatusCode, result);
  }

  [Authorize]
  [Consumes("multipart/form-data")]
  [HttpPost("profile")]
  public async Task<ActionResult<ApiResponse<UserResponseDTO>>> CreateProfile([FromForm] CreateProfileDTO dto, IFormFile? file)
  {
    var authId = User.FindFirstValue("authId");

    if (string.IsNullOrEmpty(authId) || !int.TryParse(authId, out var authIdInt))
    {
      return ApiResponse<UserResponseDTO>.Error("Token không hợp lệ.", (int)HttpStatusCode.Unauthorized);
    }

    var result = await _accountService.CreateProfileAsync(authIdInt, dto, file);
    return StatusCode(result.StatusCode, result);
  }
}