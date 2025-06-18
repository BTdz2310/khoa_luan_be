namespace learniverse_be.Models;

public class CategoriesDto
{
  public int Id { get; set; }
  public string Code { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? IconUrl { get; set; }
  public string? Color { get; set; }
}