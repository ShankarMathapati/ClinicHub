using System.ComponentModel.DataAnnotations;
using ClinicHub.Application.Common;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Doctors;

public record DoctorDto(Guid Id, string FullName, string Specialty);

public record CreateDoctorRequest(
    [Required, StringLength(150)] string FullName,
    [Required, StringLength(100)] string Specialty);

public class DoctorService(IClinicDbContext db)
{
    public Task<List<DoctorDto>> ListAsync(CancellationToken ct) =>
        db.Doctors.AsNoTracking()
            .OrderBy(d => d.FullName)
            .Select(d => new DoctorDto(d.Id, d.FullName, d.Specialty))
            .ToListAsync(ct);

    public async Task<DoctorDto> CreateAsync(CreateDoctorRequest r, CancellationToken ct)
    {
        var doctor = new Doctor(r.FullName, r.Specialty);
        db.Doctors.Add(doctor);
        await db.SaveChangesAsync(ct);
        return new DoctorDto(doctor.Id, doctor.FullName, doctor.Specialty);
    }
}
