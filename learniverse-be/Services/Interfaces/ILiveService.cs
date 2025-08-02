using learniverse_be.DTOs;
using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface ILiveService
{
  public Task<ApiResponse<List<LivestreamResponseDto>>> GetLivesByMonthAsync(int instructorId, string slug, int month, int year);
  public Task<ApiResponse<LivestreamResponseDto>> CreateLiveAsync(int instructorId, Guid courseId, LivestreamRequestDto dto);
  public Task<ApiResponse<LivestreamResponseDto>> UpdateLiveAsync(int instructorId, LivestreamRequestDto dto, Guid id);
  public Task<ApiResponse<object>> DeleteLiveAsync(int instructorId, Guid id);
  public Task<ApiResponse<LivestreamResponseDto>> ActiveLiveAsync(int instructorId, Guid id);
  public Task StartProcessing(StreamRequest request);
  public Task StopProcessing(StreamRequest request);
  public Task<bool> ValidateStreamKey(Guid streamKey);
  public Task<ApiResponse<LivestreamCourseDto>> GetLiveAsync(Guid id, int identifierId, Role role);
}