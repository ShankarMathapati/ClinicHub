using System.Net;
using System.Net.Http.Json;
using ClinicHub.Api.Controllers;
using ClinicHub.Tests.Infrastructure;

namespace ClinicHub.Tests.Api;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Login_with_valid_credentials_returns_token_for_the_users_clinic()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DemoUsers.SunriseAdmin, "Demo@12345"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestJson.Options);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal("Sunrise Clinic", body.TenantName);
        Assert.Equal("Admin", body.Role);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DemoUsers.SunriseAdmin, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/patients")]
    [InlineData("/api/doctors")]
    [InlineData("/api/appointments")]
    public async Task Protected_endpoints_reject_anonymous_requests(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Staff_cannot_create_doctors()
    {
        var staff = await factory.CreateClientForAsync(DemoUsers.SunriseStaff);

        var response = await staff.PostAsJsonAsync("/api/doctors", new { fullName = "Dr. New", specialty = "ENT" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
