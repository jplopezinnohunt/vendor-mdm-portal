using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Services;

public interface IVendorApplicationService
{
    Task<VendorApplication?> GetApplicationAsync(Guid id);
    Task ApproveApplicationAsync(Guid applicationId, Dictionary<string, object>? enrichedAttributes, bool forceSanctionsOverride = false);
    Task RejectApplicationAsync(Guid applicationId, string reason);
}

public class VendorApplicationService : IVendorApplicationService
{
    private readonly SqlDbContext _context;
    private readonly IVendorService _vendorService;
    private readonly ISanctionsScreeningService _sanctionsService;
    private readonly ISapVendorService _sapService;
    private readonly ILogger<VendorApplicationService> _logger;

    public VendorApplicationService(
        SqlDbContext context,
        IVendorService vendorService,
        ISanctionsScreeningService sanctionsService,
        ISapVendorService sapService,
        ILogger<VendorApplicationService> logger)
    {
        _context = context;
        _vendorService = vendorService;
        _sanctionsService = sanctionsService;
        _sapService = sapService;
        _logger = logger;
    }

    public async Task<VendorApplication?> GetApplicationAsync(Guid id)
    {
        return await _context.VendorApplications.FindAsync(id);
    }

    public async Task ApproveApplicationAsync(Guid applicationId, Dictionary<string, object>? enrichedAttributes, bool forceSanctionsOverride = false)
    {
        var app = await _context.VendorApplications.FindAsync(applicationId);
        if (app == null) throw new KeyNotFoundException($"Application {applicationId} not found");

        if (app.Status != "PendingReview" && app.Status != "Submitted") 
        {
            _logger.LogWarning("Attempted to approve application {Id} with status {Status}", applicationId, app.Status);
           // throw new InvalidOperationException($"Application is in status {app.Status}, cannot approve."); 
           // Relaxed check for re-runs or specific workflows, but generally should be PendingReview.
        }

        // 1. Merge Attributes (Logic moved from Controller)
        var finalAttributes = new Dictionary<string, object>();
        
        // Load existing
        if (!string.IsNullOrEmpty(app.Attributes))
        {
            try 
            {
                finalAttributes = JsonSerializer.Deserialize<Dictionary<string, object>>(app.Attributes) ?? new();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Failed to deserialize attributes for app {Id}", applicationId);
            }
        }

        // Merge overrides
        if (enrichedAttributes != null)
        {
            foreach (var kvp in enrichedAttributes)
            {
                // Update Core Columns if present in enrichment
                 if (kvp.Key.Equals("companyName", StringComparison.OrdinalIgnoreCase)) 
                    app.CompanyName = kvp.Value?.ToString() ?? app.CompanyName;
                else if (kvp.Key.Equals("taxId", StringComparison.OrdinalIgnoreCase)) 
                    app.TaxId = kvp.Value?.ToString() ?? app.TaxId;
                else if (kvp.Key.Equals("contactName", StringComparison.OrdinalIgnoreCase))
                    app.ContactName = kvp.Value?.ToString() ?? app.ContactName;
                else if (kvp.Key.Equals("email", StringComparison.OrdinalIgnoreCase))
                    app.ContactEmail = kvp.Value?.ToString() ?? app.ContactEmail;
                else
                {
                    finalAttributes[kvp.Key] = kvp.Value;
                }
            }
        }

        // Save updated state back to App (intermediate save)
        app.Attributes = JsonSerializer.Serialize(finalAttributes);
        await _context.SaveChangesAsync();


        // 2. CONSISTENCY MAPPING: Map to Canonical Vendor Entity
        // Goal: Output must be identical to "Direct Vendor Creation" structure.
        
        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            // RULE A. Structured Columns (Identity)
            LegalName = app.CompanyName,
            TaxId = app.TaxId,
            PrimaryContactEmail = app.ContactEmail,
            
            // System Metadata
            SourceSystem = "VendorPortal_Invitation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = VendorStatus.Active 
        };

        // RULE B. Semi-Structured (JSONB) - NESTED OBJECT RECONSTRUCTION
        // The Approver Form flattens address fields (street1, city, etc.)
        // We must re-nest them into an 'Address' object to keep the Schema clean.

        var vendorAttributes = new Dictionary<string, object>(finalAttributes);

        // Define the specific flattened keys to extract and nest
        var addressKeys = new[] { "street1", "street2", "street3", "street4", "postalCode", "city", "country", "houseNo" };
        var addressObj = new Dictionary<string, object>();
        bool hasAddressData = false;

        foreach (var key in addressKeys)
        {
            if (vendorAttributes.TryGetValue(key, out var val))
            {
                addressObj[key] = val;
                vendorAttributes.Remove(key); // Remove flat key to avoid duplication
                hasAddressData = true;
            }
        }

        // Check if an address object already exists (from initial enrichment) and merge/overwrite
        if (vendorAttributes.TryGetValue("address", out var existingAddrObj) && existingAddrObj is JsonElement je && je.ValueKind == JsonValueKind.Object)
        {
             // If we have mixed data, we might need complex merging. 
             // For now, if we extracted new flat data, we prioritize it (as it came from the form).
             // If we extracted nothing, we keep the existing object.
             if (!hasAddressData)
             {
                 // No new flat data, keep existing address object intact
             }
             else
             {
                 // We have new flat data. We should probably merge it with existing if needed, 
                 // but typically the form editing overwrites the state. 
                 // We'll set the new object.
                 vendorAttributes["address"] = addressObj;
             }
        }
        else if (hasAddressData)
        {
            vendorAttributes["address"] = addressObj;
        }

        vendor.Attributes = JsonSerializer.Serialize(vendorAttributes);


        // 3. SANCTIONS CHECK (Fail-Closed with Override)
        // We re-screen the FINAL vendor object before creation
        var screeningRequest = new ScreeningRequest
        {
            EntityName = vendor.LegalName,
            EntityType = "Organization", // Could map from attributes["accountGroup"]
            Address = new AddressInfo { Country = addressObj.TryGetValue("country", out var c) ? c?.ToString() : "US" }
        };

        RiskLevel risk = RiskLevel.Low;
        try 
        {
            var result = await _sanctionsService.ScreenEntityAsync(screeningRequest);
            risk = result.OverallRisk;
        }
        catch (Exception ex)
        {
            if (!forceSanctionsOverride)
            {
                _logger.LogError(ex, "Sanctions check failed during approval for {AppId}. Fail-Closed triggered.", applicationId);
                 throw new InvalidOperationException("Sanctions screening failed. Cannot proceed without override.");
            }
            _logger.LogWarning("Sanctions connectivity failed, but FORCE OVERRIDE is enabled. Proceeding.");
        }

        if ((risk == RiskLevel.High || risk == RiskLevel.Critical) && !forceSanctionsOverride)
        {
            throw new InvalidOperationException($"High Risk Sanctions Match detected ({risk}). Approval Blocked.");
        }


        // 4. CALL CORE VENDOR SERVICE (Reusable Logic)
        // This ensures Duplicate Checks, SQL Persistence, Cosmos Artifacts, and Domain Events
        // are identical to the Direct Creation flow.
        var createdVendor = await _vendorService.CreateVendorAsync(vendor, forceCreation: forceSanctionsOverride);


        // 5. SAP INTEGRATION (Placeholder/Future)
        try 
        {
            // var sapResult = await _sapService.CreateVendorAsync(new VendorCreateRequest { ... });
            // Update createdVendor.SapId = sapResult.VendorId;
            // await _vendorService.UpdateVendorAsync(createdVendor);
        }
        catch (NotImplementedException) 
        {
             _logger.LogInformation("SAP Integration skipped (Not Implemented).");
        }


        // 6. FINALIZE APPLICATION STATUS
        app.Status = "Approved";
        
        // Link the canonical Vendor ID
        // Note: VendorApplication entity might need a 'VendorId' column in future, for now we store in Attributes? 
        // Or if 'IntegrationStatus' is needed.
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Application {AppId} approved. Vendor {VendorId} created.", applicationId, createdVendor.Id);
    }

    public async Task RejectApplicationAsync(Guid applicationId, string reason)
    {
        var app = await _context.VendorApplications.FindAsync(applicationId);
        if (app == null) throw new KeyNotFoundException($"Application {applicationId} not found");

        app.Status = "Rejected";
        
        // Store reason in attributes
        var attrs = string.IsNullOrEmpty(app.Attributes) 
            ? new Dictionary<string,object>() 
            : JsonSerializer.Deserialize<Dictionary<string,object>>(app.Attributes) ?? new();
            
        attrs["rejectionReason"] = reason;
        app.Attributes = JsonSerializer.Serialize(attrs);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Application {AppId} rejected. Reason: {Reason}", applicationId, reason);
    }
}
