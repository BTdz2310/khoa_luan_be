using System.ComponentModel.DataAnnotations;

namespace learniverse_be.Models;

public class RegisterDto
{
  [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
  [MinLength(6, ErrorMessage = "Tên đăng nhập phải chứa tối thiểu 6 ký tự")]
  [MaxLength(50, ErrorMessage = "Tên đăng nhập chỉ chứa tối đa 50 ký tự")]
  [RegularExpression(@"^[a-zA-Z0-9]+$", 
    ErrorMessage = "Ký tự không được phép xuất hiện trong tên đăng nhập.")]
  public string Username { get; set; } = null!;

  [Required]
  [EmailAddress]
  public string Email { get; set; } = default!;

  [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$",
    ErrorMessage = "Mật khẩu phải có tối thiểu 8 ký tự bao gồm: chữ thường, chữ in hoa, số và ký tự đặc biệt")]
  public string Password { get; set; } = null!;
}