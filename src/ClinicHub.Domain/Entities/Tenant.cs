using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public class Tenant : Entity
{
    private Tenant() { }

    public Tenant(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
}
