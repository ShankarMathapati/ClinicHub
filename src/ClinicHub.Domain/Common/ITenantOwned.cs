namespace ClinicHub.Domain.Common;

/// <summary>
/// Marks an entity that belongs to exactly one tenant (clinic).
/// The data layer applies a global query filter and write guard to every implementation.
/// </summary>
public interface ITenantOwned
{
    Guid TenantId { get; set; }
}
