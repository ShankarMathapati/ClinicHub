using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public enum AppointmentStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2
}

public class Appointment : Entity, ITenantOwned
{
    public static readonly TimeSpan MinDuration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

    private Appointment() { }

    public Appointment(Guid patientId, Guid doctorId, DateTime startUtc, DateTime endUtc, string? reason)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
            throw new DomainException("Appointment times must be in UTC.");
        var duration = endUtc - startUtc;
        if (duration < MinDuration || duration > MaxDuration)
            throw new DomainException($"Appointment must last between {MinDuration.TotalMinutes} minutes and {MaxDuration.TotalHours} hours.");

        PatientId = patientId;
        DoctorId = doctorId;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Status = AppointmentStatus.Scheduled;
    }

    public Guid TenantId { get; set; }
    public Guid PatientId { get; private set; }
    public Patient? Patient { get; private set; }
    public Guid DoctorId { get; private set; }
    public Doctor? Doctor { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string? Reason { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
            throw new DomainException("A completed appointment cannot be cancelled.");
        Status = AppointmentStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new DomainException("A cancelled appointment cannot be completed.");
        Status = AppointmentStatus.Completed;
    }
}
