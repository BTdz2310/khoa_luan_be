using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using learniverse_be.Models;
using System.Security.Claims;
using learniverse_be.Data;
using Microsoft.EntityFrameworkCore;
using learniverse_be.Services.Interfaces;
using learniverse_be.Utils;
using Microsoft.AspNetCore.Authorization;

namespace learniverse_be.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(AppDbContext context, ILogger<AuthController> logger, IJwtTokenService jwtTokenService, IMailService mailService) : ControllerBase
{
  private readonly ILogger<AuthController> _logger = logger;
  private readonly AppDbContext _context = context;
  private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
  private readonly IMailService _mailService = mailService;

  [HttpPost("login")]
  public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto request)
  {
    // var auth = await _context.Auths.SingleOrDefaultAsync(a => a.Username == request.Username);
    var auth = await _context.Auths
      .Include(a => a.User)
      .SingleOrDefaultAsync(a => a.Username == request.Username && a.IsActive);

    if (auth == null)
    {
      return Unauthorized(new ApiResponse<object>
      {
        Data = null,
        Message = "Không có tài khoản nào khớp với thông tin bạn đã cung cấp.",
        Errors = null
      });
    }

    bool isPasswordValid = PasswordHelper.VerifyPassword(request.Password, auth.Salt, auth.PasswordHash);
    if (!isPasswordValid)
    {
      return Unauthorized(new ApiResponse<object>
      {
        Data = null,
        Message = "Không có tài khoản nào khớp với thông tin bạn đã cung cấp.",
        Errors = null
      });
    }

    var user = auth.User;

    var claims = new[]
    {
        new Claim("username", auth.Username),
        new Claim("authId", auth.Id.ToString()),
        new Claim("userId", auth.User?.Id.ToString() ?? string.Empty),
        new Claim("fullName", auth.User?.FullName ?? string.Empty),
    };

    var accessToken = _jwtTokenService.GenerateAccessToken(claims);
    var refreshToken = _jwtTokenService.GenerateRefreshToken(claims);

    var response = new LoginResponseDto
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      Auth = new AuthDto
      {
        AuthId = auth.Id,
        Username = auth.Username,
        Email = auth.Email,
        CreatedAt = auth.CreatedAt,
        User = auth.User == null ? null : new UserDto
        {
          UserId = user.Id,
          FullName = user.FullName,
        }
      }
    };

    return Ok(new
    {
      Data = response,
      Message = "Đăng nhập thành công.",
    });
  }

  [HttpPost("register")]
  [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
  [Produces("application/json")]
  public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterDto dto)
  {
    var now = DateTime.UtcNow;
    var expiredThreshold = now.AddDays(-1);

    var expiredAuth = await _context.Auths
      .Where(a => !a.IsActive
        && a.Username == dto.Username
        && a.Email == dto.Email
        && a.CreatedAt <= expiredThreshold)
      .FirstOrDefaultAsync();

    if (expiredAuth != null)
    {
      _context.Auths.Remove(expiredAuth);
      await _context.SaveChangesAsync();
    }

    if (await _context.Auths.AnyAsync(a => a.Username == dto.Username))
    {
      return BadRequest(new ApiResponse<object>(null, "Tên đăng nhập đã tồn tại.", null));
    }

    if (await _context.Auths.AnyAsync(a => a.Email == dto.Email))
    {
      return BadRequest(new ApiResponse<object>(null, "Email đã được sử dụng.", null));
    }

    var passwordHash = PasswordHelper.EncryptPassword(dto.Password);

    var auth = new Auth
    {
      Username = dto.Username,
      Email = dto.Email,
      PasswordHash = passwordHash.Hash,
      Salt = passwordHash.Salt
    };

    _context.Auths.Add(auth);
    await _context.SaveChangesAsync();

    var otpCode = OtpUtil.GenerateOtp(6);
    OtpService otpService = new OtpService(_context);
    Otp otp = await otpService.SaveOtpAsync(auth.Email, otpCode, OtpType.Register, auth.Id);

    try
    {
      _mailService.SendRegisterMail(auth.Email, otpCode);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Lỗi khi gửi mail xác thực OTP.");
      return StatusCode(500, new ApiResponse<object>(null, "Không thể gửi email xác thực. Vui lòng thử lại sau.", null));
    }

    var response = new RegisterResponseDto
    {
      AuthId = auth.Id,
      HashCode = otp.HashCode
    };

    return Ok(new ApiResponse<RegisterResponseDto>(response, "Mã xác thực otp đã được gửi đến email mà bạn cung cấp. Vui lòng kiểm tra lại email.", null));
  }

  [HttpPost("activate")]
  public async Task<ActionResult<ApiResponse<object>>> Activate([FromBody] ActivateDto dto)
  {
    var otpService = new OtpService(_context);
    var isOtpValid = await otpService.VerifyOtpAsync(new VerifyOTp { HashCode = dto.HashCode, VerificationCode = dto.VerificationCode }, OtpType.Register, true);

    if (!isOtpValid.IsValid)
    {
      return BadRequest(new ApiResponse<object>(null, "Mã xác thực không hợp lệ.", null));
    }

    var auth = await _context.Auths
      .Include(a => a.User)
      .FirstOrDefaultAsync(a => a.Id == dto.AuthId);

    if (auth == null)
    {
      return BadRequest(new ApiResponse<object>(null, "Mã xác thực không hợp lệ.", null));
    }

    auth.IsActive = true;

    _context.Auths.Update(auth);
    await _context.SaveChangesAsync();

    return Ok(new ApiResponse<object>(null, "Xác thực thành công.", null));
  }

  [HttpPost("resend-otp")]
  public async Task<ActionResult<ApiResponse<object>>> ResendOtp([FromBody] ResendOtpDto dto)
  {
    var auth = await _context.Auths.FirstOrDefaultAsync(a =>
        a.Id == dto.AuthId &&
        a.Email == dto.Email &&
        !a.IsActive);

    if (auth == null)
    {
      return BadRequest(new ApiResponse<object>(null, "Tài khoản không tồn tại.", null));
    }

    // Xóa OTP cũ nếu có
    var oldOtps = await _context.Otps
        .Where(o => o.AuthId == auth.Id && o.Type == dto.Type && !o.IsUsed)
        .ToListAsync();

    _context.Otps.RemoveRange(oldOtps);
    await _context.SaveChangesAsync();

    var otpCode = OtpUtil.GenerateOtp(6);
    var otpService = new OtpService(_context);
    var newOtp = await otpService.SaveOtpAsync(auth.Email, otpCode, dto.Type, auth.Id);

    try
    {
      _mailService.SendRegisterMail(auth.Email, otpCode);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Lỗi khi gửi lại email xác thực OTP.");
      return StatusCode(500, new ApiResponse<object>(null, "Không thể gửi lại email xác thực. Vui lòng thử lại sau.", null));
    }

    var response = new ResendOtpResponseDto
    {
      AuthId = auth.Id,
      HashCode = newOtp.HashCode
    };

    return Ok(new ApiResponse<ResendOtpResponseDto>(response, "Mã OTP mới đã được gửi đến email.", null));
  }

  [HttpPost("forget-password")]
  public async Task<ActionResult<ApiResponse<object>>> ForgetPassword([FromBody] ForgetPasswordDto dto)
  {
    var auth = await _context.Auths
      .Include(a => a.User)
      .SingleOrDefaultAsync(a => a.Email == dto.Email && a.IsActive);

    if (auth == null)
    {
      return BadRequest(new ApiResponse<object>(null, "Tài khoản không tồn tại.", null));
    }

    var otpCode = OtpUtil.GenerateOtp(6);
    OtpService otpService = new OtpService(_context);
    Otp otp = await otpService.SaveOtpAsync(dto.Email, otpCode, OtpType.ForgotPassword, auth.Id);

    try
    {
      _mailService.SendForgetMail(dto.Email, otpCode);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Lỗi khi gửi mail xác thực OTP.");
      return StatusCode(500, new ApiResponse<object>(null, "Không thể gửi email xác thực. Vui lòng thử lại sau.", null));
    }

    var response = new RegisterResponseDto
    {
      AuthId = auth.Id,
      HashCode = otp.HashCode
    };

    return Ok(new ApiResponse<RegisterResponseDto>(response, "Mã xác thực otp đã được gửi đến email mà bạn cung cấp. Vui lòng kiểm tra lại email.", null));
  }

  [HttpPost("confirm-forget-password")]
  public async Task<ActionResult<ApiResponse<object>>> ConfirmForgetPassword([FromBody] ConfirmForgetPasswordDto dto)
  {
    var otpService = new OtpService(_context);
    var isValid = await otpService.VerifyOtpAsync(new VerifyOTp { HashCode = dto.HashCode, VerificationCode = dto.VerificationCode }, OtpType.ForgotPassword, false);
    if (!isValid.IsValid)
    {
      return BadRequest(new ApiResponse<object>(null, "Mã xác thực không hợp lệ.", null));
    }

    return Ok(new ApiResponse<object>(null, "Xác thực thành công.", null));
  }

  [HttpPost("reset-password")]
  public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
  {
    var otpService = new OtpService(_context);
    var isOtpValid = await otpService.VerifyOtpAsync(new VerifyOTp { HashCode = dto.HashCode, VerificationCode = dto.VerificationCode }, OtpType.Register, true);

    if (!isOtpValid.IsValid)
    {
      return BadRequest(new ApiResponse<object>(null, "Mã xác thực không hợp lệ.", null));
    }

    var auth = await _context.Auths
      .FirstOrDefaultAsync(a => a.Id == isOtpValid.AuthId);

    if (auth == null)
    {
      return BadRequest(new ApiResponse<object>(null, "Mã xác thực không hợp lệ.", null));
    }

    var passwordHash = PasswordHelper.EncryptPassword(dto.NewPassword);

    auth.PasswordHash = passwordHash.Hash;
    auth.Salt = passwordHash.Salt;

    _context.Auths.Update(auth);
    await _context.SaveChangesAsync();

    return Ok(new ApiResponse<object>(null, "Đặt lại mật khẩu thành công.", null));
  }

  [HttpPost("refresh-token")]
  public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto)
  {
    var principal = _jwtTokenService.GetPrincipalFromExpiredToken(dto.RefreshToken);
    if (principal == null)
    {
      return Unauthorized(new ApiResponse<object>(null, "Refresh token không hợp lệ.", null));
    }

    var authId = principal.Claims.FirstOrDefault(c => c.Type == "authId")?.Value;
    if (!int.TryParse(authId, out var authIdInt))
    {
      return Unauthorized(new ApiResponse<object>(null, "Refresh token không chứa thông tin người dùng hợp lệ.", null));
    }

    var auth = await _context.Auths
      .Include(a => a.User)
      .FirstOrDefaultAsync(a => a.Id == authIdInt && a.IsActive);

    if (auth == null)
    {
      return Unauthorized(new ApiResponse<object>(null, "Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.", null));
    }

    var claims = new[]
    {
      new Claim("username", auth.Username),
      new Claim("authId", auth.Id.ToString()),
      new Claim("userId", auth.User?.Id.ToString() ?? string.Empty),
      new Claim("fullName", auth.User?.FullName ?? string.Empty),
    };

    var newAccessToken = _jwtTokenService.GenerateAccessToken(claims);
    var newRefreshToken = _jwtTokenService.GenerateRefreshToken(claims);

    var response = new RefreshTokenResponseDto
    {
      AccessToken = newAccessToken,
      RefreshToken = newRefreshToken
    };

    return Ok(new ApiResponse<RefreshTokenResponseDto>(response, "Làm mới token thành công.", null));
  }

}
