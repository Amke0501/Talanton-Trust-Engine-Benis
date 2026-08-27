namespace Talanton.Api.Models;

public class UserRoleAssignment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public Guid? SaccoId { get; set; }

    public Sacco? Sacco { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    public bool IsActive { get; set; } = true;
}