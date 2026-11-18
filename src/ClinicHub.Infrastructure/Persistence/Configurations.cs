using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Infrastructure.Persistence;

internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.Property(t => t.Name).HasMaxLength(150).IsRequired();
        b.Property(t => t.Slug).HasMaxLength(60).IsRequired();
        b.HasIndex(t => t.Slug).IsUnique();
    }
}

internal class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.Property(u => u.Email).HasMaxLength(200).IsRequired();
        b.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        b.Property(u => u.Role).HasMaxLength(20).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.HasOne<Tenant>().WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        b.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        b.Property(p => p.Phone).HasMaxLength(20);
        b.Property(p => p.Email).HasMaxLength(200);
        b.HasIndex(p => new { p.TenantId, p.LastName, p.FirstName });
        b.HasOne<Tenant>().WithMany().HasForeignKey(p => p.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> b)
    {
        b.Property(d => d.FullName).HasMaxLength(150).IsRequired();
        b.Property(d => d.Specialty).HasMaxLength(100).IsRequired();
        b.HasOne<Tenant>().WithMany().HasForeignKey(d => d.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.Property(a => a.Reason).HasMaxLength(500);
        b.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        b.HasOne(a => a.Patient).WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(a => a.Doctor).WithMany().HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tenant>().WithMany().HasForeignKey(a => a.TenantId).OnDelete(DeleteBehavior.Restrict);
        // Supports the double-booking check and the calendar range query.
        b.HasIndex(a => new { a.TenantId, a.DoctorId, a.StartUtc });
    }
}
