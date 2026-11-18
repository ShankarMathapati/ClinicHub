using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
}

public class AppUser : Entity, ITenantOwned
{
    private AppUser() { }

    public AppUser(Guid tenantId, string email, string role)
    {
        TenantId = tenantId;
        Email = email.Trim().ToLowerInvariant();
        Role = role;
    }

    public Guid TenantId { get; set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = Roles.Staff;

    public void SetPasswordHash(string hash) => PasswordHash = hash;
}
