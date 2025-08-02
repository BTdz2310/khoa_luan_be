namespace learniverse_be.Models;

public enum ModerationAction
{
  Update = 0,
  Delete = 1
}

public class LectureModeration
{
  public Guid Id { get; set; }

  public Guid? LectureId { get; set; }
  public ModerationAction ActionType { get; set; }

  public string? NewData { get; set; } = string.Empty;

  public Status Status { get; set; } = Status.Pending;
  public string? Reason { get; set; }

  public int InstructorId { get; set; }
  public int? AdminId { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? ReviewedAt { get; set; }

  public Guid SectionId { get; set; }
}
