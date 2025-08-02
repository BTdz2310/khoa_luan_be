using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace learniverse_be.Models;

public enum EnrollmentStatus
{
  Active = 0,     
  Completed = 1,  
  Cancelled = 2,
}

public class Enrollment
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [ForeignKey("User")]
  public int UserId { get; set; }
  public User User { get; set; } = default!;

  [ForeignKey("Course")]
  public Guid CourseId { get; set; }
  public Course Course { get; set; } = default!;

  public EnrollmentStatus Status { get; set; }

  public DateTime EnrolledAt { get; set; }
}