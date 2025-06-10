using learniverse_be.Models;
using Microsoft.AspNetCore.Mvc;

public static class ResponseHelper
{
  public static IActionResult Success<T>(T data, string? message = null)
  {
    return new OkObjectResult(new ApiResponse<T>(data, message));
  }

  public static IActionResult Fail(string message, List<FieldError>? errors = null)
  {
    return new BadRequestObjectResult(new ApiResponse<object>(null, message, errors));
  }

  public static IActionResult NotFound(string message)
  {
    return new NotFoundObjectResult(new ApiResponse<object>(null, message));
  }

  public static IActionResult Unauthorized(string message)
  {
    return new UnauthorizedObjectResult(new ApiResponse<object>(null, message));
  }
}
