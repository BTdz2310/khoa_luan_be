using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IAuthService
{
  Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
  Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterDto dto);
  Task<ApiResponse<object>> ActivateAsync(ActivateDto dto);
  Task<ApiResponse<ResendOtpResponseDto>> ResendOtpAsync(ResendOtpDto dto);
  Task<ApiResponse<ForgetPasswordResponseDto>> ForgetPasswordAsync(ForgetPasswordDto dto);
  Task<ApiResponse<object>> ConfirmForgetPasswordAsync(ConfirmForgetPasswordDto dto);
  Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto dto);
  Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
}