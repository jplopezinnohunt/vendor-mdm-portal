using System;
using System.Collections.Generic;

namespace VendorMdm.Shared.Ontology.AuditModels
{
    /// <summary>
    /// User-specific audit model.
    /// Defines which fields are captured in audit logs for User entities.
    /// </summary>
    public class UserAuditModel
    {
        public string SchemaVersion { get; set; } = "v1.0.0";

        // Critical Fields (MUST audit)
        public string? Email { get; set; }
        public string? Username { get; set; }
        public List<string>? Roles { get; set; }
        public bool? IsBlocked { get; set; }
        public string? Status { get; set; }

        // Standard Fields (SHOULD audit)
        public string? FullName { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int? FailedLoginAttempts { get; set; }
        public bool? TwoFactorEnabled { get; set; }

        // Metadata
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public static UserAuditModel FromEntity(Models.User user)
        {
            return new UserAuditModel
            {
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
