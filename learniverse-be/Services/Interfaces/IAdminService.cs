using learniverse_be.Models;

namespace learniverse_be.Services.Interfaces;

public interface IAdminService
{
  public Task<ApiResponse<LectureModeration>> AcceptLectureRequestAsync(Guid lectureRequestId, int adminId);
  public Task<ApiResponse<LectureModeration>> RejectLectureRequestAsync(Guid lectureRequestId, string reason, int adminId);
  public Task<ApiResponse<List<LectureModeration>>> GetLecturesRequestAsync ();
}