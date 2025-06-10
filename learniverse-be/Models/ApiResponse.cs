using System.Text.Json.Serialization;

namespace learniverse_be.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public List<FieldError>? Errors { get; set; }

    public ApiResponse(T? data = default, string? message = null, List<FieldError>? errors = null)
    {
        Data = data;
        Message = message;
        Errors = errors;
    }
}

public class FieldError
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}
