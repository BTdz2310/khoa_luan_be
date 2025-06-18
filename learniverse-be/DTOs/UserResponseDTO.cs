public class UserResponseDTO
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string? Avatar { get; set; }
    public string Bio { get; set; } = default!;
    public DateOnly BirthDate { get; set; }
    public Gender Gender { get; set; }
}