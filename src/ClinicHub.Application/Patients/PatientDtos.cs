using System.ComponentModel.DataAnnotations;

namespace ClinicHub.Application.Patients;

public record PatientDto(Guid Id, string FirstName, string LastName, DateOnly DateOfBirth, string? Phone, string? Email);

public record SavePatientRequest(
    [Required, StringLength(100)] string FirstName,
    [Required, StringLength(100)] string LastName,
    [Required] DateOnly DateOfBirth,
    [Phone, StringLength(20)] string? Phone,
    [EmailAddress, StringLength(200)] string? Email);
