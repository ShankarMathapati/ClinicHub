using ClinicHub.Application.Common;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Patients;

public class PatientService(IClinicDbContext db)
{
    public async Task<PagedResult<PatientDto>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.FirstName.Contains(term) ||
                p.LastName.Contains(term) ||
                (p.Phone != null && p.Phone.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PatientDto(p.Id, p.FirstName, p.LastName, p.DateOfBirth, p.Phone, p.Email))
            .ToListAsync(ct);

        return new PagedResult<PatientDto>(items, page, pageSize, total);
    }

    public async Task<PatientDto> GetAsync(Guid id, CancellationToken ct)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Patient), id);
        return ToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(SavePatientRequest r, CancellationToken ct)
    {
        var patient = new Patient(r.FirstName, r.LastName, r.DateOfBirth, r.Phone, r.Email);
        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);
        return ToDto(patient);
    }

    public async Task<PatientDto> UpdateAsync(Guid id, SavePatientRequest r, CancellationToken ct)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Patient), id);
        patient.Update(r.FirstName, r.LastName, r.DateOfBirth, r.Phone, r.Email);
        await db.SaveChangesAsync(ct);
        return ToDto(patient);
    }

    private static PatientDto ToDto(Patient p) => new(p.Id, p.FirstName, p.LastName, p.DateOfBirth, p.Phone, p.Email);
}
