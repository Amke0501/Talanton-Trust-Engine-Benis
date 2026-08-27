namespace Talanton.Api.Models;

public class Cooperative
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string? ContactPersonName { get; set; }

    public string? ContactPhone { get; set; }

    public string? Address { get; set; }

    public Guid? SaccoId { get; set; }

    public Sacco? Sacco { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}