using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClinicHub.Api.Controllers;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClinicHub.Tests.Infrastructure;

/// <summary>Boots the real API in memory, backed by an in-memory SQLite database seeded with two clinics.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ClinicHub", "unused-in-tests");
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-0123456789abcdef");
        builder.UseSetting("RateLimiting:LoginPermitPerMinute", "1000");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ClinicDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ClinicDbContext>>();

            _connection.Open();
            services.AddDbContext<ClinicDbContext>(o => o.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureCreated();
        DemoDataSeeder.SeedAsync(db,
            scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>(),
            scope.ServiceProvider.GetRequiredService<TimeProvider>()).GetAwaiter().GetResult();

        return host;
    }

    public async Task<HttpClient> CreateClientForAsync(string email)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, DemoDataSeeder.DemoPassword), TestJson.Options);
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(TestJson.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public static class TestJson
{
    public static readonly System.Text.Json.JsonSerializerOptions Options = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}

public static class DemoUsers
{
    public const string SunriseAdmin = "admin@sunrise.demo";
    public const string SunriseStaff = "staff@sunrise.demo";
    public const string LakesideAdmin = "admin@lakeside.demo";
}
