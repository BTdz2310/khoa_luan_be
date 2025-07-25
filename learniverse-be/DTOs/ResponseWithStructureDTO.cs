using System.Text.Json.Serialization;

namespace learniverse_be.Models;

public class ResponseWithStructureDto<T> where T : class
{
  public T? Data { get; set; }

  public List<SectionSDto> Structure { get; set; } = default!;
}