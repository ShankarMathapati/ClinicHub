using System.ComponentModel.DataAnnotations;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Appointments;

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Reason,
    AppointmentStatus Status);

public record BookAppointmentRequest(
    [Required] Guid PatientId,
    [Required] Guid DoctorId,
    [Required] DateTime StartUtc,
    [Required] DateTime EndUtc,
    [StringLength(500)] string? Reason);
