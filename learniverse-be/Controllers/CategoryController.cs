using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/category")]
public class CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService) : ControllerBase
{
  private readonly ILogger<CategoryController> _logger = logger;
  private readonly ICategoryService _categoryService = categoryService;

  [HttpGet("")]
  public async Task<ActionResult<ApiResponse<object>>> GetCategories()
  {
    var result = await _categoryService.GetCategoriesAsync();
    return StatusCode(result.StatusCode, result);
  }
}