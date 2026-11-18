using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Tests.Domain;

public class AppointmentTests
{
    private static readonly DateTime Start = new(2030, 1, 1, 9, 0, 0, DateTimeKind.Utc);

    private static Appointment NewAppointment(int minutes = 30) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Start, Start.AddMinutes(minutes), "Check-up");

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-30)]
    [InlineData(300)]
    public void Rejects_durations_outside_the_allowed_range(int minutes)
    {
        Assert.Throws<DomainException>(() => NewAppointment(minutes));
    }

    [Fact]
    public void Rejects_non_utc_times()
    {
        var local = new DateTime(2030, 1, 1, 9, 0, 0, DateTimeKind.Local);
        Assert.Throws<DomainException>(() => new Appointment(Guid.NewGuid(), Guid.NewGuid(), local, local.AddMinutes(30), null));
    }

    [Fact]
    public void New_appointments_are_scheduled()
    {
        Assert.Equal(AppointmentStatus.Scheduled, NewAppointment().Status);
    }

    [Fact]
    public void Completed_appointments_cannot_be_cancelled()
    {
        var appointment = NewAppointment();
        appointment.Complete();

        Assert.Throws<DomainException>(appointment.Cancel);
    }

    [Fact]
    public void Cancelled_appointments_cannot_be_completed()
    {
        var appointment = NewAppointment();
        appointment.Cancel();

        Assert.Throws<DomainException>(appointment.Complete);
    }
}
