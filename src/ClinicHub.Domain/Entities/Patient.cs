using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public class Patient : Entity, ITenantOwned
{
    private Patient() { }

    public Patient(string firstName, string lastName, DateOnly dateOfBirth, string? phone, string? email)
    {
        Update(firstName, lastName, dateOfBirth, phone, email);
    }

    public Guid TenantId { get; set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    public void Update(string firstName, string lastName, DateOnly dateOfBirth, string? phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("First and last name are required.");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Date of birth cannot be in the future.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}
