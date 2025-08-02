using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public class VideoChat
{
  public Guid Id { get; set; } = Guid.NewGuid();

  [ForeignKey("VideoId")]
  public Guid? VideoId { get; set; }
  public Video Video { get; set; } = default!;

  [ForeignKey("LivestreamId")]
  public Guid? LivestreamId { get; set; }
  public Livestream Livestream { get; set; } = default!;


  [ForeignKey("UserId")]
  public User? User { get; set; }
  public int? UserId { get; set; }

  [ForeignKey("InstructorId")]
  public Instructor? Instructor { get; set; }
  public int? InstructorId { get; set; }

  public string Content { get; set; } = default!;

  public double? VideoTimestamp { get; set; }

  public DateTime CreatedAt { get; set; }

  public Guid? ParentId { get; set; }
  public VideoChat? Parent { get; set; }
}
