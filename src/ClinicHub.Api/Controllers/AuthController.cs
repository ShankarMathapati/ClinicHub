using System.ComponentModel.DataAnnotations;
using ClinicHub.Api.Auth;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Api.Controllers;

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string Email, string Role, string TenantName);

public record MeResponse(string Email, string Role, Guid TenantId, string TenantName);

[ApiController]
[Route("api/auth")]
public class AuthController(ClinicDbContext db, IPasswordHasher<AppUser> hasher, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Login happens before a tenant is known, so this is the one place that bypasses the tenant filter.
        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");

        var tenant = await db.Tenants.FirstAsync(t => t.Id == user.TenantId, ct);
        var token = tokens.Create(user, tenant);
        return Ok(new LoginResponse(token.Token, token.ExpiresAtUtc, user.Email, user.Role, tenant.Name));
    }

    [HttpGet("me")]
    public ActionResult<MeResponse> Me() => new MeResponse(
        User.FindFirst(ClinicClaims.Email)?.Value ?? string.Empty,
        User.FindFirst(ClinicClaims.Role)?.Value ?? string.Empty,
        Guid.Parse(User.FindFirst(ClinicClaims.TenantId)!.Value),
        User.FindFirst(ClinicClaims.TenantName)?.Value ?? string.Empty);
}
