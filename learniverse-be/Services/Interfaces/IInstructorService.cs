using learniverse_be.DTOs;
using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IInstructorService
{
  public Task<ApiResponse<InstructorResponseDto>> CreateInstructorAsync(int authId, CreateInstructorDto dto, IFormFile? file);
}