namespace learniverse_be.Models;

public class UserDto
{
  public int UserId { get; set; }
  public string FullName { get; set; } = default!;
  public string? Avatar { get; set; }
  public string Bio { get; set; } = default!;
  public Gender Gender { get; set; }
  public DateOnly BirthDate { get; set; }
  public List<int> Interestings { get; set; } = null!; 
}