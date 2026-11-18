using ClinicHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Persistence;

/// <summary>Seeds two demo clinics so tenant isolation can be seen in action.</summary>
public static class DemoDataSeeder
{
    public const string DemoPassword = "Demo@12345";

    public static async Task SeedAsync(ClinicDbContext db, IPasswordHasher<AppUser> hasher, TimeProvider clock, CancellationToken ct = default)
    {
        if (await db.Tenants.AnyAsync(ct)) return;

        var today = clock.GetUtcNow().UtcDateTime.Date;

        await SeedClinicAsync(db, hasher, today, "Sunrise Clinic", "sunrise",
            [("Dr. Ananya Rao", "General Medicine"), ("Dr. Vikram Shetty", "Pediatrics")],
            [("Rahul", "Kumar", new DateOnly(1988, 4, 12)), ("Priya", "Nair", new DateOnly(1995, 9, 3)), ("Arjun", "Hegde", new DateOnly(2012, 1, 27))],
            ct);

        await SeedClinicAsync(db, hasher, today, "Lakeside Health", "lakeside",
            [("Dr. Meera Iyer", "Cardiology")],
            [("Sneha", "Patil", new DateOnly(1979, 6, 18)), ("Karthik", "Reddy", new DateOnly(1990, 11, 30))],
            ct);
    }

    private static async Task SeedClinicAsync(
        ClinicDbContext db, IPasswordHasher<AppUser> hasher, DateTime today,
        string name, string slug,
        (string Name, string Specialty)[] doctors,
        (string First, string Last, DateOnly Dob)[] patients,
        CancellationToken ct)
    {
        var tenant = new Tenant(name, slug);
        db.Tenants.Add(tenant);

        var admin = new AppUser(tenant.Id, $"admin@{slug}.demo", Roles.Admin);
        admin.SetPasswordHash(hasher.HashPassword(admin, DemoPassword));
        var staff = new AppUser(tenant.Id, $"staff@{slug}.demo", Roles.Staff);
        staff.SetPasswordHash(hasher.HashPassword(staff, DemoPassword));
        db.Users.AddRange(admin, staff);

        var doctorEntities = doctors.Select(d => new Doctor(d.Name, d.Specialty) { TenantId = tenant.Id }).ToList();
        var patientEntities = patients.Select(p => new Patient(p.First, p.Last, p.Dob, null, null) { TenantId = tenant.Id }).ToList();
        db.Doctors.AddRange(doctorEntities);
        db.Patients.AddRange(patientEntities);

        // A couple of appointments tomorrow, 04:00 and 05:00 UTC (09:30 / 10:30 IST).
        for (var i = 0; i < Math.Min(2, patientEntities.Count); i++)
        {
            var start = today.AddDays(1).AddHours(4 + i);
            db.Appointments.Add(new Appointment(patientEntities[i].Id, doctorEntities[0].Id, start, start.AddMinutes(30), "Consultation")
            {
                TenantId = tenant.Id
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
