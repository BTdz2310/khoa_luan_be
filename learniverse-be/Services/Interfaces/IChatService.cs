using learniverse_be.DTOs;
using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IChatService
{
  public Task<ApiResponse<VideoChatResponseDto>> CreateChatVideo(int identifierId, VideoChatRequestDto dto, Role role);
}