using System.ComponentModel.DataAnnotations;

namespace learniverse_be.DTOs;

public class CreateInstructorDto
{
  [Required]
  [MaxLength(150)]
  public string DisplayName { get; set; } = default!;

  [Required]
  [MaxLength(150)]
  public string Headline { get; set; } = default!;

  public string? Bio { get; set; } = default!;

  [Required]
  [MinLength(1, ErrorMessage = "Phải có ít nhất một ngôn ngữ.")]
  public List<string> Languages { get; set; } = new();

  [Required]
  [MinLength(1, ErrorMessage = "Phải có ít nhất một lĩnh vực chuyên môn.")]
  public List<string> Expertise { get; set; } = new();

  [Required]
  public Degree Degree { get; set; }

  [Range(1, 100, ErrorMessage = "Số năm kinh nghiệm phải từ 1 đến 100.")]
  public int ExperienceYears { get; set; }
}
