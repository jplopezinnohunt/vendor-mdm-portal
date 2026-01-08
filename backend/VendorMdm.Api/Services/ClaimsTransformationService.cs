using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace VendorMdm.Api.Services
{
    public class ClaimsTransformationService : IClaimsTransformation
    {
        // Define Group IDs (Mapped to specific Azure AD Group Names)
        // Note: In production, these should be strict GUIDs. For now, we match on Group Names or IDs provided by config.
        private const string GROUP_REQUESTOR = "UNESCO-MoUV-Requestors";
        private const string GROUP_VENDOR_UNIT = "UNESCO-MoUV-VendorUnit";
        private const string GROUP_BFM = "UNESCO-MoUV-BFM";
        private const string GROUP_ADMIN = "UNESCO-MoUV-Admins";

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            // If not authenticated or already transformed, skip
            if (identity == null || !identity.IsAuthenticated)
            {
                return Task.FromResult(principal);
            }

            // Avoid adding duplicate claims if already processed
            if (identity.HasClaim(c => c.Type == "RBAC_PROCESSED"))
            {
                return Task.FromResult(principal);
            }

            // We look for 'groups' claim. 
            // NOTE: Azure AD usually sends Group IDs (GUIDs). 
            // If the token is configured to emit Group Names, the value will be the name.
            // We assume the values below might appear in the 'groups' or 'roles' collection.
            var groupClaims = principal.FindAll("groups").Select(c => c.Value).ToHashSet();
            
            // Also check 'roles' claim if App Roles are used
            var roleClaims = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet();
            
            var newClaims = new List<Claim>();
            bool hasInternalRole = false;

            // Map Groups to Roles
            if (groupClaims.Contains(GROUP_REQUESTOR) || roleClaims.Contains(GROUP_REQUESTOR))
            {
                newClaims.Add(new Claim(ClaimTypes.Role, "Requestor"));
                hasInternalRole = true;
            }
            if (groupClaims.Contains(GROUP_VENDOR_UNIT) || roleClaims.Contains(GROUP_VENDOR_UNIT))
            {
                newClaims.Add(new Claim(ClaimTypes.Role, "VendorUnit"));
                newClaims.Add(new Claim(ClaimTypes.Role, "Approver")); // VendorUnit implies Approver
                hasInternalRole = true;
            }
            if (groupClaims.Contains(GROUP_BFM) || roleClaims.Contains(GROUP_BFM))
            {
                newClaims.Add(new Claim(ClaimTypes.Role, "BFM"));
                newClaims.Add(new Claim(ClaimTypes.Role, "Approver")); // BFM implies Approver
                hasInternalRole = true;
            }
            if (groupClaims.Contains(GROUP_ADMIN) || roleClaims.Contains(GROUP_ADMIN))
            {
                newClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
                newClaims.Add(new Claim(ClaimTypes.Role, "Approver")); // Admin implies Approver
                hasInternalRole = true;
            }

            // Vendor Logic: If authenticated but NO internal group found, assign Vendor role
            // This covers the "Vendor Role... act as per invitation" requirement. 
            // They are authenticated users without internal privileges.
            if (!hasInternalRole)
            {
                 newClaims.Add(new Claim(ClaimTypes.Role, "Vendor"));
            }

            // Mark as processed
            newClaims.Add(new Claim("RBAC_PROCESSED", "true"));

            // Use "AddClaims" to append to existing identity
            identity.AddClaims(newClaims);

            return Task.FromResult(principal);
        }
    }
}
