using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public class Doctor : Entity, ITenantOwned
{
    private Doctor() { }

    public Doctor(string fullName, string specialty)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Doctor name is required.");
        FullName = fullName.Trim();
        Specialty = specialty.Trim();
    }

    public Guid TenantId { get; set; }
    public string FullName { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
}
