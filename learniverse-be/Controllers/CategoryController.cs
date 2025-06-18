using System.Security.Claims;
using learniverse_be.Data;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("/api/category")]
public class CategoryController(AppDbContext context, ILogger<CategoryController> logger, IJwtTokenService jwtTokenService, IMailService mailService) : ControllerBase
{
  private readonly ILogger<CategoryController> _logger = logger;
  private readonly AppDbContext _context = context;
  private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
  private readonly IMailService _mailService = mailService;

  [HttpGet("")]
  public async Task<ActionResult<ApiResponse<object>>> GetCategories()
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

    return Ok(new ApiResponse<List<CategoriesDto>>(categories, "Lấy thông tin thành công.", null));
  }
}