namespace learniverse_be.Models;

public class MailSettings
{
  public string EmailId { get; set; } = default!;
  public string Name { get; set; } = default!;
  public string UserName { get; set; } = default!;
  public string Password { get; set; } = default!;
  public string Host { get; set; } = default!;
  public int Port { get; set; }
  public bool UseSSL { get; set; }
}