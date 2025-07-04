using System.Net;
using System.Security.Claims;
using learniverse_be.Data;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using learniverse_be.Utils;
using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
  private readonly AppDbContext _context;
  private readonly IJwtTokenService _jwtService;
  private readonly IMailService _mailService;
  private readonly ILogger<AuthService> _logger;

  public AuthService(AppDbContext context, IJwtTokenService jwtService, IMailService mailService, ILogger<AuthService> logger)
  {
    _context = context;
    _jwtService = jwtService;
    _mailService = mailService;
    _logger = logger;
  }

  public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
  {
    var auth = await _context.Auths.Include(a => a.User)
      .SingleOrDefaultAsync(a => a.Username == dto.Username && a.IsActive);

    if (auth == null || !PasswordHelper.VerifyPassword(dto.Password, auth.Salt, auth.PasswordHash))
    {
      return ApiResponse<LoginResponseDto>.Error("Không có tài khoản nào khớp với thông tin bạn đã cung cấp.", (int)HttpStatusCode.Unauthorized);
    }

    var claims = new[]
    {
      new Claim("username", auth.Username),
      new Claim("authId", auth.Id.ToString()),
      new Claim("userId", auth.User?.Id.ToString() ?? string.Empty),
      new Claim("fullName", auth.User?.FullName ?? string.Empty),
    };

    var response = new LoginResponseDto
    {
      AccessToken = _jwtService.GenerateAccessToken(claims),
      RefreshToken = _jwtService.GenerateRefreshToken(claims),
      Auth = new AuthDto
      {
        AuthId = auth.Id,
        Username = auth.Username,
        Email = auth.Email,
        CreatedAt = auth.CreatedAt,
        User = auth.User == null ? null : new UserDto
        {
          UserId = auth.User.Id,
          FullName = auth.User.FullName,
          Avatar = auth.User.Avatar,
          Bio = auth.User.Bio,
          BirthDate = auth.User.BirthDate,
          Gender = auth.User.Gender
        }
      }
    };

    return ApiResponse<LoginResponseDto>.Success(response, "Đăng nhập thành công");
  }

  public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterDto dto)
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
      return ApiResponse<RegisterResponseDto>.Error("Tên đăng nhập đã tồn tại.", (int)HttpStatusCode.BadRequest);
    }

    if (await _context.Auths.AnyAsync(a => a.Email == dto.Email))
    {
      return ApiResponse<RegisterResponseDto>.Error("Email đã được sử dụng.", (int)HttpStatusCode.BadRequest);
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
      return ApiResponse<RegisterResponseDto>.Error("Không thể gửi email xác thực. Vui lòng thử lại sau.", (int)HttpStatusCode.InternalServerError);
    }

    var response = new RegisterResponseDto
    {
      AuthId = auth.Id,
      HashCode = otp.HashCode
    };

    return ApiResponse<RegisterResponseDto>.Success(response, "Mã xác thực otp đã được gửi đến email mà bạn cung cấp. Vui lòng kiểm tra lại email.", (int)HttpStatusCode.Created);
  }

  public async Task<ApiResponse<object>> ActivateAsync(ActivateDto dto)
  {
    var otpService = new OtpService(_context);
    var isOtpValid = await otpService.VerifyOtpAsync(new VerifyOTp { HashCode = dto.HashCode, VerificationCode = dto.VerificationCode }, OtpType.Register, true);

    if (!isOtpValid.IsValid)
    {
      return ApiResponse<object>.Error("Mã xác thực không hợp lệ.", (int)HttpStatusCode.BadRequest);
    }

    var auth = await _context.Auths
      .Include(a => a.User)
      .FirstOrDefaultAsync(a => a.Id == dto.AuthId);

    if (auth == null)
    {
      return ApiResponse<object>.Error("Mã xác thực không hợp lệ.", (int)HttpStatusCode.BadRequest);
    }

    auth.IsActive = true;

    _context.Auths.Update(auth);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Xác thực thành công.");
  }

  public async Task<ApiResponse<ResendOtpResponseDto>> ResendOtpAsync(ResendOtpDto dto)
  {
    var auth = await _context.Auths.FirstOrDefaultAsync(a =>
      a.Id == dto.AuthId &&
      a.Email == dto.Email &&
      !a.IsActive);

    if (auth == null)
    {
      return ApiResponse<ResendOtpResponseDto>.Error("Tài khoản không tồn tại.", (int)HttpStatusCode.BadRequest);
    }

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
      return ApiResponse<ResendOtpResponseDto>.Error("Không thể gửi lại email xác thực. Vui lòng thử lại sau.", (int)HttpStatusCode.InternalServerError);
    }

    var response = new ResendOtpResponseDto
    {
      AuthId = auth.Id,
      HashCode = newOtp.HashCode
    };

    return ApiResponse<ResendOtpResponseDto>.Success(response, "Mã OTP mới đã được gửi đến email.", (int)HttpStatusCode.OK);
  }

  public async Task<ApiResponse<ForgetPasswordResponseDto>> ForgetPasswordAsync(ForgetPasswordDto dto)
  {
    var auth = await _context.Auths
      .Include(a => a.User)
      .SingleOrDefaultAsync(a => a.Email == dto.Email && a.IsActive);

    if (auth == null)
    {
      return ApiResponse<ForgetPasswordResponseDto>.Error("Tài khoản không tồn tại.", (int)HttpStatusCode.BadRequest);
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
      return ApiResponse<ForgetPasswordResponseDto>.Error("Không thể gửi email xác thực. Vui lòng thử lại sau.", (int)HttpStatusCode.InternalServerError);
    }

    var response = new ForgetPasswordResponseDto
    {
      AuthId = auth.Id,
      HashCode = otp.HashCode
    };

    return ApiResponse<ForgetPasswordResponseDto>.Success(response, "Mã xác thực otp đã được gửi đến email mà bạn cung cấp. Vui lòng kiểm tra lại email.", (int)HttpStatusCode.OK);
  }

  public async Task<ApiResponse<object>> ConfirmForgetPasswordAsync(ConfirmForgetPasswordDto dto)
  {
    var otpService = new OtpService(_context);
    var isValid = await otpService.VerifyOtpAsync(new VerifyOTp { HashCode = dto.HashCode, VerificationCode = dto.VerificationCode }, OtpType.ForgotPassword, false);
    if (!isValid.IsValid)
    {
      return ApiResponse<object>.Error("Mã xác thực không hợp lệ.", (int)HttpStatusCode.BadRequest);
    }

    return ApiResponse<object>.Success(null, "Xác thực thành công.");
  }

  public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto dto)
  {
    var otpService = new OtpService(_context);
    var isOtpValid = await otpService.VerifyOtpAsync(new VerifyOTp { HashCode = dto.HashCode, VerificationCode = dto.VerificationCode }, OtpType.Register, true);

    if (!isOtpValid.IsValid)
    {
      return ApiResponse<object>.Error("Mã xác thực không hợp lệ.", (int)HttpStatusCode.BadRequest);
    }

    var auth = await _context.Auths
      .FirstOrDefaultAsync(a => a.Id == isOtpValid.AuthId);

    if (auth == null)
    {
      return ApiResponse<object>.Error("Mã xác thực không hợp lệ.", (int)HttpStatusCode.BadRequest);
    }

    var passwordHash = PasswordHelper.EncryptPassword(dto.NewPassword);

    auth.PasswordHash = passwordHash.Hash;
    auth.Salt = passwordHash.Salt;

    _context.Auths.Update(auth);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Đặt lại mật khẩu thành công.");
  }

  public async Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
  {
    var principal = _jwtService.GetPrincipalFromExpiredToken(dto.RefreshToken);
    if (principal == null)
    {
      return ApiResponse<RefreshTokenResponseDto>.Error("Refresh token không hợp lệ.", (int)HttpStatusCode.Unauthorized);
    }

    var authId = principal.Claims.FirstOrDefault(c => c.Type == "authId")?.Value;
    if (!int.TryParse(authId, out var authIdInt))
    {
      return ApiResponse<RefreshTokenResponseDto>.Error("Refresh token không chứa thông tin người dùng hợp lệ.", (int)HttpStatusCode.Unauthorized);
    }

    var auth = await _context.Auths
      .Include(a => a.User)
      .FirstOrDefaultAsync(a => a.Id == authIdInt && a.IsActive);

    if (auth == null)
    {
      return ApiResponse<RefreshTokenResponseDto>.Error("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.", (int)HttpStatusCode.Unauthorized);
    }

    var claims = new[]
    {
      new Claim("username", auth.Username),
      new Claim("authId", auth.Id.ToString()),
      new Claim("userId", auth.User?.Id.ToString() ?? string.Empty),
      new Claim("fullName", auth.User?.FullName ?? string.Empty),
    };

    var newAccessToken = _jwtService.GenerateAccessToken(claims);
    var newRefreshToken = _jwtService.GenerateRefreshToken(claims);

    var response = new RefreshTokenResponseDto
    {
      AccessToken = newAccessToken,
      RefreshToken = newRefreshToken
    };

    return ApiResponse<RefreshTokenResponseDto>.Success(response, "Làm mới token thành công.");
  }
}
