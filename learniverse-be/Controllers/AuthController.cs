using Microsoft.AspNetCore.Mvc;
using learniverse_be.Models;
using System.Security.Claims;
using learniverse_be.Data;
using Microsoft.EntityFrameworkCore;
using learniverse_be.Services.Interfaces;
using learniverse_be.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace learniverse_be.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
  private readonly ILogger<AuthController> _logger = logger;
  private readonly IAuthService _authService = authService;

  [SwaggerOperation(Summary = "Đăng nhập")]
  [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("login")]
  public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
  {
    var result = await _authService.LoginAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Đăng ký")]
  [HttpPost("register")]
  [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
  [Consumes("application/json")]
  [Produces("application/json")]
  public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterDto dto)
  {
    var result = await _authService.RegisterAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Xác thực OTP sau khi đăng ký")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("activate")]
  public async Task<ActionResult<ApiResponse<object>>> Activate([FromBody] ActivateDto dto)
  {
    var result = await _authService.ActivateAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Gửi lại mã xác thực OTP sau khi đăng ký")]
  [ProducesResponseType(typeof(ApiResponse<ResendOtpResponseDto>), StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("resend-otp")]
  public async Task<ActionResult<ApiResponse<ResendOtpResponseDto>>> ResendOtp([FromBody] ResendOtpDto dto)
  {
    var result = await _authService.ResendOtpAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Quên mật khẩu")]
  [ProducesResponseType(typeof(ApiResponse<ForgetPasswordResponseDto>), StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("forget-password")]
  public async Task<ActionResult<ApiResponse<ForgetPasswordResponseDto>>> ForgetPassword([FromBody] ForgetPasswordDto dto)
  {
    var result = await _authService.ForgetPasswordAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Kiểm tra OTP quên mật khẩu")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("confirm-forget-password")]
  public async Task<ActionResult<ApiResponse<object>>> ConfirmForgetPassword([FromBody] ConfirmForgetPasswordDto dto)
  {
    var result = await _authService.ConfirmForgetPasswordAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Đặt lại mật khẩu")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("reset-password")]
  public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
  {
    var result = await _authService.ResetPasswordAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

  [SwaggerOperation(Summary = "Refresh access token")]
  [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponseDto>), StatusCodes.Status200OK)]
  [Consumes("application/json")]
  [Produces("application/json")]
  [HttpPost("refresh-token")]
  public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto)
  {
    var result = await _authService.RefreshTokenAsync(dto);
    return StatusCode(result.StatusCode, result);
  }

}
