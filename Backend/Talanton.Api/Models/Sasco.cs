namespace Talanton.Api.Models;

public class Sacco
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";

    public string? ContactEmail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}