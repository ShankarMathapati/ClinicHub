using ClinicHub.Application.Appointments;
using ClinicHub.Application.Doctors;
using ClinicHub.Application.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<PatientService>();
        services.AddScoped<DoctorService>();
        services.AddScoped<AppointmentService>();
        return services;
    }
}
