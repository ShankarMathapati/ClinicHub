using ClinicHub.Application.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClinicHub")
            ?? throw new InvalidOperationException("Connection string 'ClinicHub' is not configured.");

        services.AddDbContext<ClinicDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<IClinicDbContext>(sp => sp.GetRequiredService<ClinicDbContext>());
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        return services;
    }
}
