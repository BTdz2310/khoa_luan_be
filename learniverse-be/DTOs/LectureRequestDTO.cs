using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace learniverse_be.Models;

public class LectureRequestDto
{
  [Required]
  [JsonPropertyName("id")]
  public Guid Id { get; set; } = default!;

  [Required]
  [JsonPropertyName("title")]
  public string Title { get; set; } = default!;

  [Required]
  [JsonPropertyName("order")]
  public decimal Order { get; set; } = default!;

  [Required]
  [JsonPropertyName("sectionId")]
  public Guid SectionId { get; set; } = default!;

  [JsonPropertyName("isPreviewable")]
  public bool IsPreviewable { get; set; } = false;
  [JsonPropertyName("description")]
  public string? Description { get; set; }
}