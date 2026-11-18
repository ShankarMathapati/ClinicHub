using ClinicHub.Application.Common;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Appointments;

public class AppointmentService(IClinicDbContext db, TimeProvider clock)
{
    public async Task<List<AppointmentDto>> ListAsync(DateTime fromUtc, DateTime toUtc, Guid? doctorId, CancellationToken ct)
    {
        var query = db.Appointments.AsNoTracking()
            .Where(a => a.StartUtc < toUtc && a.EndUtc > fromUtc);
        if (doctorId is not null) query = query.Where(a => a.DoctorId == doctorId);

        return await query
            .OrderBy(a => a.StartUtc)
            .Select(a => new AppointmentDto(
                a.Id,
                a.PatientId,
                a.Patient!.FirstName + " " + a.Patient.LastName,
                a.DoctorId,
                a.Doctor!.FullName,
                a.StartUtc,
                a.EndUtc,
                a.Reason,
                a.Status))
            .ToListAsync(ct);
    }

    public async Task<AppointmentDto> BookAsync(BookAppointmentRequest r, CancellationToken ct)
    {
        var start = DateTime.SpecifyKind(r.StartUtc.ToUniversalTime(), DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(r.EndUtc.ToUniversalTime(), DateTimeKind.Utc);

        if (start < clock.GetUtcNow().UtcDateTime)
            throw new DomainException("Appointments cannot be booked in the past.");

        // Tenant query filters make a patient or doctor from another clinic look like it does not exist.
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == r.PatientId, ct)
            ?? throw new NotFoundException(nameof(Patient), r.PatientId);
        var doctor = await db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == r.DoctorId, ct)
            ?? throw new NotFoundException(nameof(Doctor), r.DoctorId);

        var doctorBusy = await db.Appointments.AnyAsync(a =>
            a.DoctorId == r.DoctorId &&
            a.Status == AppointmentStatus.Scheduled &&
            a.StartUtc < end && start < a.EndUtc, ct);
        if (doctorBusy)
            throw new ConflictException($"{doctor.FullName} already has an appointment in that time slot.");

        var patientBusy = await db.Appointments.AnyAsync(a =>
            a.PatientId == r.PatientId &&
            a.Status == AppointmentStatus.Scheduled &&
            a.StartUtc < end && start < a.EndUtc, ct);
        if (patientBusy)
            throw new ConflictException("The patient already has an appointment in that time slot.");

        var appointment = new Appointment(r.PatientId, r.DoctorId, start, end, r.Reason);
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        return new AppointmentDto(appointment.Id, patient.Id, $"{patient.FirstName} {patient.LastName}",
            doctor.Id, doctor.FullName, start, end, appointment.Reason, appointment.Status);
    }

    public Task CancelAsync(Guid id, CancellationToken ct) => ChangeStatusAsync(id, a => a.Cancel(), ct);

    public Task CompleteAsync(Guid id, CancellationToken ct) => ChangeStatusAsync(id, a => a.Complete(), ct);

    private async Task ChangeStatusAsync(Guid id, Action<Appointment> change, CancellationToken ct)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException(nameof(Appointment), id);
        change(appointment);
        await db.SaveChangesAsync(ct);
    }
}
