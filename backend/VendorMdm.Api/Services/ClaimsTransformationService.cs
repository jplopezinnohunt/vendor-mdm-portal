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
            var groupClaims = principal.FindAll("groups").ToList();
            var roles = new List<string>();

            foreach (var groupClaim in groupClaims)
            {
                var groupId = groupClaim.Value;

                // Map specific Azure AD Groups to Roles
                switch (groupId)
                {
                    case "UNESCO-MoUV-Vendor":
                        roles.Add("Vendor");
                        break;
                    case "UNESCO-MoUV-Requestors":
                        roles.Add("Requestor");
                        break;
                    case "UNESCO-MoUV-VendorUnit":
                        roles.Add("VendorUnit");
                        roles.Add("Approver"); // VendorUnit can approve
                        break;
                    case "UNESCO-MoUV-BFM":
                        roles.Add("BFM");
                        roles.Add("Approver"); // BFM can approve
                        break;
                    case "UNESCO-MoUV-Admins":
                        roles.Add("Admin");
                        roles.Add("Approver"); // Admins can approve
                        break;
                }
            }

            // Default: If no internal group, assign Vendor role
            if (roles.Count == 0)
            {
                roles.Add("Vendor");
            }

            var newClaims = new List<Claim>();
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
