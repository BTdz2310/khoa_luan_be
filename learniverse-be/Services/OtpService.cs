using learniverse_be.Data;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class OtpService : IOtpService
{
  private readonly AppDbContext _dbContext;

  public OtpService(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Otp> SaveOtpAsync(string email, string otpCode, OtpType type, int? authId)
  {

    var hashedOtpCode = PasswordHelper.EncryptPassword(otpCode);

    var otp = new Otp
    {
      Email = email,
      VerificationCode = hashedOtpCode.Hash,
      Salt = hashedOtpCode.Salt,
      Type = type,
      IsUsed = false,
      AuthId = authId
    };

    _dbContext.Otps.Add(otp);
    await _dbContext.SaveChangesAsync();

    return otp;
  }

  public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOTp dto, OtpType type, bool changeUse)
  {
    var otp = await _dbContext.Otps
      .Where(x => x.HashCode == dto.HashCode && !x.IsUsed)
      .OrderByDescending(x => x.CreatedAt)
      .FirstOrDefaultAsync();

    if (otp == null || otp.ExpirationTime < DateTime.UtcNow)
      return new VerifyOtpResponse { IsValid = false };

    var isPasswordValid = PasswordHelper.VerifyPassword(dto.VerificationCode, otp.Salt, otp.VerificationCode);
    if (!isPasswordValid)
      return new VerifyOtpResponse { IsValid = false };

    if (changeUse)
    {
      otp.IsUsed = true;
      await _dbContext.SaveChangesAsync();
    }

    return new VerifyOtpResponse { IsValid = true, AuthId = otp.AuthId };
  }
}
