using ClinicHub.Application.Common;

namespace ClinicHub.Api.Auth;

/// <summary>Reads the tenant from the signed JWT, so a client can never choose another clinic's data.</summary>
public class HttpTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    public Guid? TenantId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirst(ClinicClaims.TenantId)?.Value, out var id) ? id : null;
}

public static class ClinicClaims
{
    public const string TenantId = "tenant_id";
    public const string TenantName = "tenant_name";
    public const string Role = "role";
    public const string Email = "email";
}
