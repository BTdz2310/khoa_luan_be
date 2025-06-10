namespace learniverse_be.Models;

public class MailData
{
  public string EmailToId { get; set; } = default!;
  public string EmailSubject { get; set; } = default!;
  public string EmailBody { get; set; } = default!;
}