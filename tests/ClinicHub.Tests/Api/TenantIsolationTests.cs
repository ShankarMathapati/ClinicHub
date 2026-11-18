using System.Net;
using System.Net.Http.Json;
using ClinicHub.Application.Common;
using ClinicHub.Application.Doctors;
using ClinicHub.Application.Patients;
using ClinicHub.Tests.Infrastructure;

namespace ClinicHub.Tests.Api;

/// <summary>The core guarantee of the platform: one clinic can never read or change another clinic's data.</summary>
public class TenantIsolationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Each_clinic_only_sees_its_own_patients()
    {
        var sunrise = await factory.CreateClientForAsync(DemoUsers.SunriseAdmin);
        var lakeside = await factory.CreateClientForAsync(DemoUsers.LakesideAdmin);

        var sunrisePatients = await sunrise.GetFromJsonAsync<PagedResult<PatientDto>>("/api/patients", TestJson.Options);
        var lakesidePatients = await lakeside.GetFromJsonAsync<PagedResult<PatientDto>>("/api/patients", TestJson.Options);

        Assert.Contains(sunrisePatients!.Items, p => p.LastName == "Kumar");
        Assert.DoesNotContain(sunrisePatients.Items, p => p.LastName == "Patil");
        Assert.Contains(lakesidePatients!.Items, p => p.LastName == "Patil");
        Assert.DoesNotContain(lakesidePatients.Items, p => p.LastName == "Kumar");
    }

    [Fact]
    public async Task A_clinic_cannot_read_or_update_another_clinics_patient_by_id()
    {
        var sunrise = await factory.CreateClientForAsync(DemoUsers.SunriseAdmin);
        var lakeside = await factory.CreateClientForAsync(DemoUsers.LakesideAdmin);

        var create = await lakeside.PostAsJsonAsync("/api/patients",
            new SavePatientRequest("Isolated", "Patient", new DateOnly(2000, 1, 1), null, null));
        var lakesidePatient = await create.Content.ReadFromJsonAsync<PatientDto>(TestJson.Options);

        var read = await sunrise.GetAsync($"/api/patients/{lakesidePatient!.Id}");
        var update = await sunrise.PutAsJsonAsync($"/api/patients/{lakesidePatient.Id}",
            new SavePatientRequest("Hijacked", "Patient", new DateOnly(2000, 1, 1), null, null));

        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);

        var stillIntact = await lakeside.GetFromJsonAsync<PatientDto>($"/api/patients/{lakesidePatient.Id}", TestJson.Options);
        Assert.Equal("Isolated", stillIntact!.FirstName);
    }

    [Fact]
    public async Task A_clinic_cannot_book_with_another_clinics_doctor()
    {
        var sunrise = await factory.CreateClientForAsync(DemoUsers.SunriseAdmin);
        var lakeside = await factory.CreateClientForAsync(DemoUsers.LakesideAdmin);

        var lakesideDoctor = (await lakeside.GetFromJsonAsync<List<DoctorDto>>("/api/doctors", TestJson.Options))!.First();
        var sunrisePatient = (await sunrise.GetFromJsonAsync<PagedResult<PatientDto>>("/api/patients", TestJson.Options))!.Items.First();

        var start = DateTime.UtcNow.Date.AddDays(10).AddHours(6);
        var response = await sunrise.PostAsJsonAsync("/api/appointments", new
        {
            patientId = sunrisePatient.Id,
            doctorId = lakesideDoctor.Id,
            startUtc = start,
            endUtc = start.AddMinutes(30)
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
