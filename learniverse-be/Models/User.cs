using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learniverse_be.Models;

public class User
{
  [Key]
  public int Id { get; set; }
  
  [Required, MaxLength(150)]
  public string FullName { get; set; } = default!;
  
  [ForeignKey("Auth")]
  public int AuthId { get; set; }

  public Auth Auth { get; set; } = default!;
}