using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface ICategoryService
{
  public Task<ApiResponse<List<CategoriesDto>>> GetCategoriesAsync();
}