using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IAccountService
{
  Task<ApiResponse<AuthDto>> GetProfileAsync(int authId);
  Task<ApiResponse<UserResponseDTO>> CreateProfileAsync(int authId, CreateProfileDTO dto, IFormFile? file);
}