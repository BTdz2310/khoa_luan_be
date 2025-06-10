namespace learniverse_be.Services.Interfaces;

public interface IMailService
{
  bool SendRegisterMail(string emailToId, string otpCode);
  bool SendForgetMail(string emailToId, string otpCode);
}