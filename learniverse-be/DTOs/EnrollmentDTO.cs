using learniverse_be.Models;

public class EnrollmentDto
{
  public Guid Id { get; set; }
  public int UserId { get; set; }
  public Guid CourseId { get; set; }
  public EnrollmentStatus Status { get; set; }
  public DateTime EnrolledAt { get; set; }

}