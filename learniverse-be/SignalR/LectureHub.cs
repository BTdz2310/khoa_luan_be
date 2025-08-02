using System.Security.Claims;
using System.Text.Json;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace learniverse_be.Hubs;

[Authorize]
public class LectureHub : Hub
{
  private readonly IChatService _chatService;

  public LectureHub(IChatService chatService)
  {
    _chatService = chatService;
  }

  public async Task JoinGroup(string videoId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, $"video-{videoId}");
  }

  public async Task LeaveGroup(string videoId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video-{videoId}");
  }

  public async Task JoinLiveGroup(string liveId)
  {
    Console.WriteLine($"JoinLiveGroup: live-{liveId}");
    await Groups.AddToGroupAsync(Context.ConnectionId, $"live-{liveId}");
  }

  public async Task LeaveLiveGroup(string liveId)
  {
    Console.WriteLine($"LeaveLiveGroup: live-{liveId}");
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"live-{liveId}");
  }

  public async Task SendComment(VideoChatRequestDto dto, Role role)
  {
    string? claimValue = role == Role.User
      ? Context.User?.FindFirstValue("userId")
      : Context.User?.FindFirstValue("instructorId");

    if (string.IsNullOrEmpty(claimValue) || !int.TryParse(claimValue, out var identifierId))
    {
      return;
    }

    var chat = await _chatService.CreateChatVideo(identifierId, dto, role);

    if (dto.VideoId != null)
    {
      await Clients.Group($"video-{chat.Data.VideoId}").SendAsync("ReceiveComment", chat);
    }
    else
    {
      await Clients.Group($"live-{chat.Data.LivestreamId}").SendAsync("ReceiveLiveComment", chat);
    }
    ;
  }
}
