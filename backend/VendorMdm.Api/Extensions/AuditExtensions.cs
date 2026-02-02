using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;

namespace VendorMdm.Api.Extensions;

/// <summary>
/// Extension methods for DbContext to enable automatic audit logging.
/// Implements Pattern 16: Audit Trail & Temporal.
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Saves changes with automatic audit logging.
    /// </summary>
    public static async Task<int> SaveChangesWithAuditAsync(
        this SqlDbContext context,
        AuditInterceptor auditInterceptor,
        CancellationToken cancellationToken = default)
    {
        // Capture audit logs before saving
        var auditLogs = auditInterceptor.CaptureAuditLogs(context);

        // Save changes
        var result = await context.SaveChangesAsync(cancellationToken);

        // Add audit logs to context and save them
        if (auditLogs.Any())
        {
            context.AuditLogs.AddRange(auditLogs);
            await context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
