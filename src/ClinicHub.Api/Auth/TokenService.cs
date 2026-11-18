using System.ComponentModel.DataAnnotations;
using System.Text;
using ClinicHub.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClinicHub.Api.Auth;

public class JwtOptions
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;

    /// <summary>HMAC signing key, at least 32 characters. Supplied via user-secrets, env vars or Key Vault, never committed.</summary>
    [Required, MinLength(32)] public string Key { get; set; } = string.Empty;

    [Range(5, 1440)] public int ExpiryMinutes { get; set; } = 60;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

public record AccessToken(string Token, DateTime ExpiresAtUtc);

public class TokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken Create(AppUser user, Tenant tenant)
    {
        var expires = clock.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpiryMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(_options.SigningKey(), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [ClinicClaims.Email] = user.Email,
                [ClinicClaims.Role] = user.Role,
                [ClinicClaims.TenantId] = tenant.Id.ToString(),
                [ClinicClaims.TenantName] = tenant.Name
            }
        };
        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
