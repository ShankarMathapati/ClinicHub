using System.Reflection;
using ClinicHub.Application.Common;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Persistence;

/// <summary>
/// Single database, shared schema multi-tenancy:
/// every <see cref="ITenantOwned"/> row carries a TenantId, reads are filtered by a global
/// query filter and writes are stamped and guarded in <see cref="SaveChangesAsync(CancellationToken)"/>.
/// </summary>
public class ClinicDbContext(DbContextOptions<ClinicDbContext> options, ITenantContext tenantContext)
    : DbContext(options), IClinicDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // Referenced by the query filters; EF re-evaluates it for every query on this context instance.
    public Guid? CurrentTenantId => tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicDbContext).Assembly);

        var applyFilter = typeof(ClinicDbContext).GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType)))
        {
            applyFilter.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantOwned
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<TEntity>().HasIndex(e => e.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GuardTenantWrites();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        GuardTenantWrites();
        return base.SaveChanges();
    }

    private void GuardTenantWrites()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantOwned>())
        {
            switch (entry.State)
            {
                case EntityState.Added when entry.Entity.TenantId == Guid.Empty:
                    entry.Entity.TenantId = CurrentTenantId
                        ?? throw new InvalidOperationException("Cannot create tenant data without a tenant context.");
                    break;
                case EntityState.Modified or EntityState.Deleted:
                    if (entry.Property(e => e.TenantId).IsModified || entry.Entity.TenantId != CurrentTenantId)
                        throw new InvalidOperationException("Cross-tenant write blocked.");
                    break;
            }
        }
    }
}
