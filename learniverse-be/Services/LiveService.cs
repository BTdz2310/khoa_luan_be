using System.Diagnostics;
using System.Net;
using learniverse_be.Data;
using learniverse_be.DTOs;
using learniverse_be.Models;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using learniverse_be.Hubs;
using System.Text.Json;

namespace learniverse_be.Services;

public class LiveService : ILiveService
{
  private readonly AppDbContext _context;
  private readonly ILogger<ILiveService> _logger;
  private readonly IDtoService _dtoService;
  private readonly ICourseService _courseService;
  private readonly IS3Service _s3Service;
  private readonly Dictionary<string, CancellationTokenSource> _activeStreams = new();
  private static readonly Dictionary<Guid, Process> _ffmpegProcesses = new();

  private readonly IHubContext<LectureHub> _lectureHubContext;


  public LiveService(AppDbContext context, ILogger<ILiveService> logger, IDtoService dtoService, ICourseService courseService, IS3Service s3Service, IHubContext<LectureHub> lectureHubContext)
  {
    _context = context;
    _logger = logger;
    _dtoService = dtoService;
    _courseService = courseService;
    _s3Service = s3Service;
    _lectureHubContext = lectureHubContext;
  }

  public async Task<ApiResponse<List<LivestreamResponseDto>>> GetLivesByMonthAsync(int instructorId, string slug, int month, int year)
  {
    var streams = await _context.Livestreams.Include(s => s.Course).ThenInclude(c => c.Category).Include(s => s.Course).ThenInclude(c => c.Instructor).Where(s => s.Course.Slug == slug).ToListAsync();
    var result = streams.Select(s => _dtoService.LivestreamToDto(s)).ToList();
    return ApiResponse<List<LivestreamResponseDto>>.Success(result);
  }

  public async Task<bool> ValidateStreamKey(Guid streamKey)
  {
    var stream = await _context.Livestreams.Include(s => s.Course).FirstOrDefaultAsync(s => s.StreamKey == streamKey);

    if (stream == null) return false;

    var now = DateTimeOffset.UtcNow;
    var startMargin = stream.StartTime.AddMinutes(-5);
    var endMargin = stream.EndTime.AddMinutes(5);

    if (now < startMargin || now > endMargin) return false;

    if (stream.Status != LivestreamStatus.Live) return false;

    return true;
  }

  public async Task<ApiResponse<LivestreamCourseDto>> GetLiveAsync(Guid id, int identifierId, Role role)
  {
    var live = await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Category).Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.Id == id);

    if (live == null)
    {
      return ApiResponse<LivestreamCourseDto>.Error("Không tìm thấy sự kiện.", (int)HttpStatusCode.BadRequest);
    }

    if (await _courseService.ValidatePermissionCourse(live.Course, identifierId, role) == false)
    {
      return ApiResponse<LivestreamCourseDto>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    var chatsDto = new List<VideoChatResponseDto>();
    var chats = await _context.VideoChats.Include(c => c.Video).ThenInclude(c => c.Lecture).ThenInclude(l => l.Section).ThenInclude(s => s.Course).Include(c => c.User).Include(c => c.Instructor).Where(c => c.LivestreamId == live.Id).OrderBy(c => c.CreatedAt).ToListAsync();
    chatsDto = chats.Select(c => _dtoService.VideoChatToDto(c)).ToList();

    return ApiResponse<LivestreamCourseDto>.Success(
      new LivestreamCourseDto
      {
        Course = _dtoService.CourseToDto(live.Course),
        Livestream = _dtoService.LivestreamToDto(live),
        Chats = chatsDto
      },
      "Lấy thành công thông tin sự kiện"
    );

  }

  public async Task<ApiResponse<LivestreamResponseDto>> CreateLiveAsync(int instructorId, Guid courseId, LivestreamRequestDto dto)
  {
    var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

    if (course == null)
    {
      return ApiResponse<LivestreamResponseDto>.Error("Không tìm thấy khoá học.", (int)HttpStatusCode.BadRequest);
    }

    if (course.InstructorId != instructorId)
    {
      return ApiResponse<LivestreamResponseDto>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    var livestream = new Livestream
    {
      CourseId = courseId,
      Title = dto.Title,
      StartTime = dto.StartTime,
      EndTime = dto.EndTime,
    };

    await _context.Livestreams.AddAsync(livestream);
    await _context.SaveChangesAsync();

    await _context.Entry(livestream)
      .Reference(l => l.Course)
      .Query()
      .Include(c => c.Category)
      .Include(c => c.Instructor)
      .LoadAsync();

    return ApiResponse<LivestreamResponseDto>.Success(_dtoService.LivestreamToDto(livestream), "Tạo sự kiện thành công");
  }

  public async Task<ApiResponse<LivestreamResponseDto>> UpdateLiveAsync(int instructorId, LivestreamRequestDto dto, Guid id)
  {
    var live = await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Category).Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.Id == id);

    if (live == null)
    {
      return ApiResponse<LivestreamResponseDto>.Error("Không tìm thấy sự kiện.", (int)HttpStatusCode.BadRequest);
    }

    if (live.Course.InstructorId != instructorId)
    {
      return ApiResponse<LivestreamResponseDto>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    live.Title = dto.Title;
    live.StartTime = dto.StartTime;
    live.EndTime = dto.EndTime;

    await _context.SaveChangesAsync();
    return ApiResponse<LivestreamResponseDto>.Success(_dtoService.LivestreamToDto(live), "Cập nhật sự kiện thành công");
  }

  public async Task<ApiResponse<object>> DeleteLiveAsync(int instructorId, Guid id)
  {
    var live = await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Category).Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.Id == id);

    if (live == null)
    {
      return ApiResponse<object>.Error("Không tìm thấy sự kiện.", (int)HttpStatusCode.BadRequest);
    }

    if (live.Course.InstructorId != instructorId)
    {
      return ApiResponse<object>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    _context.Livestreams.Remove(live);
    await _context.SaveChangesAsync();

    return ApiResponse<object>.Success(null, "Xóa sự kiện.");
  }

  public async Task<ApiResponse<LivestreamResponseDto>> ActiveLiveAsync(int instructorId, Guid id)
  {
    var live = await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Category).Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.Id == id);

    if (live == null)
    {
      return ApiResponse<LivestreamResponseDto>.Error("Không tìm thấy sự kiện.", (int)HttpStatusCode.BadRequest);
    }

    if (live.Course.InstructorId != instructorId)
    {
      return ApiResponse<LivestreamResponseDto>.Error("Không đủ quyền thực hiện hành động.", (int)HttpStatusCode.Unauthorized);
    }

    live.Status = LivestreamStatus.Live;

    await _context.SaveChangesAsync();
    return ApiResponse<LivestreamResponseDto>.Success(_dtoService.LivestreamToDto(live), "Active livestream thành công");
  }

  public async Task StartProcessing(StreamRequest request)
  {

    await Task.Delay(5000);

    var live = await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Category).Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.StreamKey == request.Name);
    live.PlaybackUrl = "https://learniverse-s3-hou.s3.ap-southeast-2.amazonaws.com/lives/" + request.Name + "/playlist.m3u8";
    await _context.SaveChangesAsync();
    Console.WriteLine($"---Update: {JsonSerializer.Serialize(_dtoService.LivestreamToDto(live))}");

    var cancellationToken = new CancellationTokenSource();
    _activeStreams[request.Name.ToString()] = cancellationToken;

    _ = Task.Run(() => ProcessStream(request.Name, live, cancellationToken.Token));

    _logger.LogInformation($"Started processing stream: {request.Name}");
    Console.WriteLine($"Started processing stream: {request.Name}");

  }

  public async Task StopProcessing(StreamRequest request)
  {
    _logger.LogInformation($"🛑 Stopping processing stream: {request.Name}");

    if (_ffmpegProcesses.TryGetValue(request.Name, out var process))
    {
      _logger.LogInformation($"Killing ffmpeg process for {request.Name}, PID: {process.Id}");
      process.Kill(true);
      process.WaitForExit();
      _ffmpegProcesses.Remove(request.Name);
    }

    var live = await _context.Livestreams.Include(l => l.Course).ThenInclude(c => c.Category).Include(l => l.Course).ThenInclude(c => c.Instructor).FirstOrDefaultAsync(l => l.StreamKey == request.Name);
    if (live != null)
    {
      live.Status = LivestreamStatus.Finished;
      await _context.SaveChangesAsync();
      await _lectureHubContext
        .Clients
        .Group($"live-{live.Id}")
        .SendAsync("UpdateLive", _dtoService.LivestreamToDto(live));
      _logger.LogInformation($"📝 Updated livestream status to Finished for {request.Name}");
    }
  }

  private async Task MonitorFiles(string localPath)
  {
    try
    {
      if (!Directory.Exists(localPath))
      {
        _logger.LogWarning($"📁 Directory doesn't exist: {localPath}");
        return;
      }

      var files = Directory.GetFiles(localPath);

      if (files.Length == 0)
      {
        _logger.LogInformation($"📁 No files found in: {localPath}");
      }
      else
      {
        _logger.LogInformation($"📊 Found {files.Length} files:");
        foreach (var file in files)
        {
          var info = new FileInfo(file);
          _logger.LogInformation($"  📄 {info.Name} - {info.Length} bytes - {info.LastWriteTime}");
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error monitoring files");
    }
  }

  private async Task WaitForFirstSegment(string localPath, CancellationToken cancellationToken)
  {
    var timeout = TimeSpan.FromSeconds(30);
    var startTime = DateTime.UtcNow;

    while (DateTime.UtcNow - startTime < timeout && !cancellationToken.IsCancellationRequested)
    {
      if (Directory.Exists(localPath))
      {
        var files = Directory.GetFiles(localPath, "*.ts");
        if (files.Length > 0)
        {
          _logger.LogInformation($"✅ First segment created: {Path.GetFileName(files[0])}");
          return;
        }
      }

      await Task.Delay(1000, cancellationToken);
    }

    _logger.LogWarning("⚠️ Timeout waiting for first segment");
  }

  private async Task StartMonitorTask(string localPath, CancellationToken cancellationToken)
  {
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        await MonitorFiles(localPath);
        await Task.Delay(5000, cancellationToken);
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("🛑 Monitor task cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Error in monitor task");
    }
  }

  private async Task SyncLocalFilesToS3(string localPath, Guid liveId)
  {
    try
    {
      if (!Directory.Exists(localPath))
      {
        _logger.LogWarning($"📁 Sync skipped - directory doesn't exist: {localPath}");
        return;
      }

      var files = Directory.GetFiles(localPath);

      if (files.Length == 0)
      {
        _logger.LogInformation("📤 No files to sync to S3");
        return;
      }

      _logger.LogInformation($"📤 Starting sync of {files.Length} files to S3...");

      var syncTasks = files.Select(async filePath =>
      {
        var fileName = Path.GetFileName(filePath);

        // Skip temporary files
        if (fileName.EndsWith(".tmp") || fileName.StartsWith("."))
        {
          return;
        }

        await UploadFileWithRetrySimple(filePath, fileName, liveId);
      });

      await Task.WhenAll(syncTasks);
      _logger.LogInformation($"✅ Sync batch completed");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Error syncing files to S3");
    }
  }

  private async Task ProcessStream(Guid streamKey, Livestream live, CancellationToken cancellationToken)
  {
    var tempPath = Path.GetTempPath();
    var localOutputPath = Path.Combine(tempPath, "hls", streamKey.ToString());
    Directory.CreateDirectory(localOutputPath);

    _logger.LogInformation($"🔍 Stream Key: {streamKey}");
    _logger.LogInformation($"🔍 Live ID: {streamKey}");
    _logger.LogInformation($"🔍 Local output path: {localOutputPath}");

    var segmentPath = Path.Combine(localOutputPath, "segment_%03d.ts");
    var playlistPath = Path.Combine(localOutputPath, "playlist.m3u8");

    var ffmpegProcess = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "ffmpeg",
        // FIX: Improved HLS settings for live streaming
        Arguments = $"-v debug -i rtmp://localhost:1936/process/{streamKey} " +
          "-c:v libx264 -preset ultrafast " +
          "-tune zerolatency " +
          "-c:a aac -b:a 128k " +
          "-f hls " +
          "-hls_time 1.5 " +                
          "-hls_list_size 4 " +            
          "-hls_flags program_date_time+delete_segments+append_list+omit_endlist " + 
          "-hls_allow_cache 0 " +           
          "-hls_segment_type mpegts " +     
          "-start_number 0 " +              
          "-avoid_negative_ts make_zero " + 
          "-fflags +genpts+flush_packets " + 
          "-flush_packets 1 " +            
          $"-hls_segment_filename \"{segmentPath}\" " +
          $"\"{playlistPath}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      }
    };
    _ffmpegProcesses[streamKey] = ffmpegProcess;

    try
    {
      _logger.LogInformation($"🚀 FFmpeg command: {ffmpegProcess.StartInfo.Arguments}");

      // _logger.LogInformation($"📦 FFmpeg exited: {ffmpegProcess.HasExited}, ExitCode: {ffmpegProcess.ExitCode}");

      ffmpegProcess.Start();

      ffmpegProcess.OutputDataReceived += (sender, e) =>
      {
        if (!string.IsNullOrEmpty(e.Data))
        {
          _logger.LogInformation($"FFmpeg Output: {e.Data}");
        }
      };

      ffmpegProcess.ErrorDataReceived += (sender, e) =>
      {
        if (!string.IsNullOrEmpty(e.Data))
        {
          _logger.LogError($"FFmpeg Error: {e.Data}");
        }
      };

      ffmpegProcess.BeginErrorReadLine();
      ffmpegProcess.BeginOutputReadLine();

      // Wait for first segment
      await WaitForFirstSegment(localOutputPath, cancellationToken);

      // Start continuous sync task
      var syncTask = StartContinuousSyncTask(localOutputPath, streamKey, cancellationToken);
      var monitorTask = StartMonitorTask(localOutputPath, cancellationToken);

      await _lectureHubContext
        .Clients
        .Group($"live-{live.Id}")
        .SendAsync("UpdateLive", _dtoService.LivestreamToDto(live));

      // Keep process running
      while (!ffmpegProcess.HasExited && !cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(500, cancellationToken); // Check more frequently
      }

      // Final sync
      await SyncLocalFilesToS3(localOutputPath, streamKey);
      await Task.WhenAll(syncTask, monitorTask);

      _logger.LogInformation($"✅ Stream {streamKey} processing completed successfully");
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation($"🛑 Stream {streamKey} processing cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"❌ Error processing stream {streamKey}");
    }
    finally
    {
      // await CleanupProcess(ffmpegProcess, streamKey);
    }
  }

  // FIX: More aggressive sync for live streaming
  private async Task StartContinuousSyncTask(string localPath, Guid liveId, CancellationToken cancellationToken)
  {
    var lastSyncedFiles = new HashSet<string>();

    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        await SyncNewFilesToS3(localPath, liveId, lastSyncedFiles);
        await Task.Delay(1000, cancellationToken);
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("🛑 Continuous sync task cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Error in continuous sync task");
    }
  }

  private async Task SyncNewFilesToS3(string localPath, Guid liveId, HashSet<string> lastSyncedFiles)
  {
    try
    {
      if (!Directory.Exists(localPath))
        return;

      var files = Directory.GetFiles(localPath)
          .Where(f => !Path.GetFileName(f).StartsWith(".") && !f.EndsWith(".tmp"))
          .ToArray();

      if (files.Length == 0)
      {
        if (lastSyncedFiles.Count > 0)
        {
          _logger.LogInformation("📤 No files found - stream may have ended");
        }
        return;
      }

      var newOrModifiedFiles = new List<string>();

      foreach (var filePath in files)
      {
        var fileName = Path.GetFileName(filePath);
        var fileInfo = new FileInfo(filePath);
        var fileKey = $"{fileName}_{fileInfo.Length}_{fileInfo.LastWriteTime.Ticks}";

        if (!lastSyncedFiles.Contains(fileKey))
        {
          newOrModifiedFiles.Add(filePath);
          lastSyncedFiles.Add(fileKey);
        }
      }

      if (newOrModifiedFiles.Count > 0)
      {
        _logger.LogInformation($"📤 Syncing {newOrModifiedFiles.Count} new/modified files...");

        var playlistFiles = newOrModifiedFiles.Where(f => f.EndsWith(".m3u8")).ToList();
        var segmentFiles = newOrModifiedFiles.Where(f => f.EndsWith(".ts")).ToList();

        foreach (var file in segmentFiles)
        {
          await UploadFileWithRetrySimple(file, Path.GetFileName(file), liveId);
        }

        foreach (var file in playlistFiles)
        {
          await UploadFileWithRetrySimple(file, Path.GetFileName(file), liveId);
        }
      }

      if (lastSyncedFiles.Count > 100)
      {
        var oldEntries = lastSyncedFiles.Take(lastSyncedFiles.Count - 50).ToList();
        foreach (var entry in oldEntries)
        {
          lastSyncedFiles.Remove(entry);
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Error syncing new files to S3");
    }
  }

  private async Task UploadFileWithRetrySimple(string filePath, string fileName, Guid liveId, int maxRetries = 3)
  {
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
      try
      {
        var uploadUrl = _s3Service.GenerateUploadUrlWithAutoContentType(liveId, fileName);

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        using var fileStream = File.OpenRead(filePath);
        using var content = new StreamContent(fileStream);

        if (fileName.EndsWith(".ts"))
        {
          content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp2t");
        }
        else if (fileName.EndsWith(".m3u8"))
        {
          content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.apple.mpegurl");
        }

        var response = await httpClient.PutAsync(uploadUrl, content);

        if (response.IsSuccessStatusCode)
        {
          _logger.LogInformation($"✅ Uploaded {fileName} ({fileStream.Length} bytes)");
          return;
        }
        else
        {
          var responseBody = await response.Content.ReadAsStringAsync();
          _logger.LogWarning($"⚠️ Upload failed for {fileName}: {response.StatusCode} - {response.ReasonPhrase}");
          _logger.LogWarning($"Response body: {responseBody}");
        }
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"⚠️ Upload exception for {fileName} - attempt {attempt}: {ex.Message}");
        if (ex.InnerException != null)
        {
          _logger.LogWarning($"Inner exception: {ex.InnerException.Message}");
        }
      }

      if (attempt < maxRetries)
      {
        await Task.Delay(1000 * attempt); 
      }
    }

    _logger.LogError($"❌ Failed to upload {fileName} after {maxRetries} attempts");
  }
}