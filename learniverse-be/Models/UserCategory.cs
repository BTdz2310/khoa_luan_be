namespace learniverse_be.Models;

public class UserCategory
{
  public int UserId { get; set; }
  public User User { get; set; }

  public int CategoryId { get; set; }
  public Category Category { get; set; }

  public DateTime InterestedAt { get; set; }
}
