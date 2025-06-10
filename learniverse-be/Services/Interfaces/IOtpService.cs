using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IOtpService
{
  Task<Otp> SaveOtpAsync(string email, string otpCode, OtpType type, int? authId);
  Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOTp dto, OtpType type, bool changeUse);
}
