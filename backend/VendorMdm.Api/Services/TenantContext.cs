namespace VendorMdm.Api.Services;

/// <summary>
/// Tenant context for multi-tenancy support (Pattern 15).
/// Provides tenant isolation for different UN agencies.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantCode { get; } // "UNDP", "UNICEF", "WHO", etc.
    string? TenantName { get; }
}

/// <summary>
/// Implementation of tenant context from HTTP headers or claims.
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            // Try to get from claims first
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "tenant_id");
            
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                return tenantId;
            }

            // Try from header (for API-to-API calls)
            var tenantHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (tenantHeader != null && Guid.TryParse(tenantHeader, out var headerTenantId))
            {
                return headerTenantId;
            }

            return null; // Global tenant
        }
    }

    public string? TenantCode
    {
        get
        {
            var tenantCodeClaim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "tenant_code");
            
            return tenantCodeClaim?.Value ?? 
                   _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Code"].FirstOrDefault();
        }
    }

    public string? TenantName
    {
        get
        {
            var tenantNameClaim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "tenant_name");
            
            return tenantNameClaim?.Value ?? TenantCode;
        }
    }
}
