using System.Net;
using System.Net.Http.Json;
using ClinicHub.Application.Appointments;
using ClinicHub.Application.Common;
using ClinicHub.Application.Doctors;
using ClinicHub.Application.Patients;
using ClinicHub.Domain.Entities;
using ClinicHub.Tests.Infrastructure;

namespace ClinicHub.Tests.Api;

public class AppointmentBookingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<(HttpClient Client, DoctorDto Doctor, List<PatientDto> Patients)> ArrangeAsync()
    {
        var client = await factory.CreateClientForAsync(DemoUsers.SunriseAdmin);
        var doctors = await client.GetFromJsonAsync<List<DoctorDto>>("/api/doctors", TestJson.Options);
        var patients = await client.GetFromJsonAsync<PagedResult<PatientDto>>("/api/patients", TestJson.Options);
        // Use the second doctor, who has no seeded appointments.
        return (client, doctors!.Single(d => d.FullName == "Dr. Vikram Shetty"), patients!.Items.ToList());
    }

    private static object Booking(Guid patientId, Guid doctorId, DateTime start, int minutes = 30) =>
        new { patientId, doctorId, startUtc = start, endUtc = start.AddMinutes(minutes), reason = "Check-up" };

    [Fact]
    public async Task Booking_a_free_slot_succeeds_and_shows_in_the_calendar()
    {
        var (client, doctor, patients) = await ArrangeAsync();
        var start = DateTime.UtcNow.Date.AddDays(20).AddHours(5);

        var response = await client.PostAsJsonAsync("/api/appointments", Booking(patients[0].Id, doctor.Id, start));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var calendar = await client.GetFromJsonAsync<List<AppointmentDto>>(
            $"/api/appointments?fromUtc={start.AddDays(-1):O}&toUtc={start.AddDays(1):O}&doctorId={doctor.Id}", TestJson.Options);
        var booked = Assert.Single(calendar!);
        Assert.Equal(AppointmentStatus.Scheduled, booked.Status);
        Assert.Equal(doctor.FullName, booked.DoctorName);
    }

    [Fact]
    public async Task Double_booking_a_doctor_returns_409()
    {
        var (client, doctor, patients) = await ArrangeAsync();
        var start = DateTime.UtcNow.Date.AddDays(21).AddHours(5);
        await client.PostAsJsonAsync("/api/appointments", Booking(patients[0].Id, doctor.Id, start));

        // Overlaps the last 15 minutes of the first appointment.
        var response = await client.PostAsJsonAsync("/api/appointments", Booking(patients[1].Id, doctor.Id, start.AddMinutes(15)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task A_cancelled_slot_can_be_booked_again()
    {
        var (client, doctor, patients) = await ArrangeAsync();
        var start = DateTime.UtcNow.Date.AddDays(22).AddHours(5);
        var first = await (await client.PostAsJsonAsync("/api/appointments", Booking(patients[0].Id, doctor.Id, start)))
            .Content.ReadFromJsonAsync<AppointmentDto>(TestJson.Options);

        var cancel = await client.PostAsync($"/api/appointments/{first!.Id}/cancel", null);
        var rebook = await client.PostAsJsonAsync("/api/appointments", Booking(patients[1].Id, doctor.Id, start));

        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        Assert.Equal(HttpStatusCode.Created, rebook.StatusCode);
    }

    [Fact]
    public async Task Booking_in_the_past_returns_400()
    {
        var (client, doctor, patients) = await ArrangeAsync();

        var response = await client.PostAsJsonAsync("/api/appointments",
            Booking(patients[0].Id, doctor.Id, DateTime.UtcNow.AddDays(-1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
