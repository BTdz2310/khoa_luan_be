using learniverse_be.Data;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace learniverse_be.Services;

public class CategoryService : ICategoryService
{
  private readonly AppDbContext _context;
  public CategoryService(AppDbContext context)
  {
    _context = context;
  }

  public async Task<ApiResponse<List<CategoriesDto>>> GetCategoriesAsync()
  {
    var categories = await _context.Categories
      .Select(c => new CategoriesDto
      {
        Id = c.Id,
        Code = c.Code,
        Name = c.Name,
        Description = c.Description,
        IconUrl = c.IconUrl,
        Color = c.Color
      })
      .ToListAsync();

    return ApiResponse<List<CategoriesDto>>.Success(categories, "Lấy thông tin thành công.");
  }

  public CategoriesDto CategotyToDto(Category category)
  {
    return new CategoriesDto
    {
      Id = category.Id,
      Code = category.Code,
      Name = category.Name,
      Description = category.Description,
      IconUrl = category.IconUrl,
      Color = category.Color
    };
  }
}