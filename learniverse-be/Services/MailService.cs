using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;

public class MailService : IMailService
{
  MailSettings Mail_Settings = null;

  public MailService(IOptions<MailSettings> options)
  {
    Mail_Settings = options.Value;
  }

  private bool SendMail(MailData Mail_Data)
  {
    try
    {
      //MimeMessage - a class from Mimekit
      MimeMessage email_Message = new MimeMessage();
      MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
      email_Message.From.Add(email_From);
      MailboxAddress email_To = new MailboxAddress("dkowakd", Mail_Data.EmailToId);
      email_Message.To.Add(email_To);
      email_Message.Subject = Mail_Data.EmailSubject;
      BodyBuilder emailBodyBuilder = new BodyBuilder();
      // emailBodyBuilder.TextBody = Mail_Data.EmailBody;
      emailBodyBuilder.HtmlBody = Mail_Data.EmailBody;
      email_Message.Body = emailBodyBuilder.ToMessageBody();
      //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
      SmtpClient MailClient = new SmtpClient();
      MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
      MailClient.Authenticate(Mail_Settings.UserName, Mail_Settings.Password);
      MailClient.Send(email_Message);
      MailClient.Disconnect(true);
      MailClient.Dispose();
      return true;
    }
    catch (Exception ex)
    {
      // Exception Details
      Console.WriteLine("Lỗi gửi mail: " + ex.Message);
      return false;
    }
  }

  public bool SendRegisterMail(string emailToId, string otpCode)
  {
    return SendMail(new MailData
    {
      EmailToId = emailToId,
      EmailSubject = "[Learniverse] Mã xác thực đăng ký tài khoản của bạn",
      EmailBody = $@"
        <html>
        <body>
            <h2>Chào mừng bạn đến với Learniverse!</h2>
            <p>Đây là mã xác minh của bạn:</p>
            <h3 style='color: blue;'>{otpCode}</h3>
            <p>Mã sẽ hết hạn sau 5 phút.</p>
            <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
        </body>
        </html>"
    });
  }

  public bool SendForgetMail(string emailToId, string otpCode)
  {
    return SendMail(new MailData
    {
      EmailToId = emailToId,
      EmailSubject = "[Learniverse] Mã xác thực quên mật khẩu tài khoản của bạn",
      EmailBody = $@"
        <html>
        <body>
            <h2>Chào mừng bạn đến với Learniverse!</h2>
            <p>Đây là mã xác minh của bạn:</p>
            <h3 style='color: blue;'>{otpCode}</h3>
            <p>Mã sẽ hết hạn sau 5 phút.</p>
            <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
        </body>
        </html>"
    });
  }
}