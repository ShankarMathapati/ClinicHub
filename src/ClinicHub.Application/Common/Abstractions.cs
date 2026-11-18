using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Common;

/// <summary>Resolves the tenant of the current request. Null means no tenant (anonymous).</summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
}

public interface IClinicDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Appointment> Appointments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
