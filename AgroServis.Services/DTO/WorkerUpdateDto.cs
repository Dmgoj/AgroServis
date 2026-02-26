namespace AgroServis.Services.DTO;

public record WorkerUpdateDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Email { get; init; } = "";
    public string? PhoneNumber { get; init; }
    public string? Position { get; init; }
}