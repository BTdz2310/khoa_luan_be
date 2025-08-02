using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class CreateCourseDto
{
  [Required]
  public int CategoryId { get; set; }

  [Required(ErrorMessage = "Vui lòng nhập tên khoá học")]
  public string Title { get; set; } = string.Empty;

  [Required(ErrorMessage = "Vui lòng nhập slug")]
  [RegularExpression(@"^[a-zA-Z]+(-[a-zA-Z]+)*$", ErrorMessage = "Slug chỉ bao gồm chữ cái và dấu '-' (không bắt đầu/kết thúc bằng '-')")]
  public string Slug { get; set; } = string.Empty;

  [Required(ErrorMessage = "Vui lòng nhập mô tả")]
  public string ShortDescription { get; set; } = string.Empty;

  [Required]
  public Language Language { get; set; }

  [Required]
  public Level Level { get; set; }

  [Required]
  [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số lớn hơn hoặc bằng 0")]
  public decimal Price { get; set; }

  public List<string> Requirements { get; set; } = new();

  public List<string> LearningObjectives { get; set; } = new();
}
