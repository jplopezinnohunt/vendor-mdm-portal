# Ontology Implementation Usage Example

This document demonstrates how the **Ontology Model** applies to our current **Vendor** and **Invitation** entities.

## 1. The Scenario: "Can we invite this Vendor?"

**Business Rule**: 
1.  If the Vendor is from an **Event**, they don't need a Tax ID yet.
2.  If the Vendor is **Direct**, they MUST have a Tax ID.
3.  If the Vendor is **High Risk** (Sanctions), they cannot be invited.

---

## 2. Current Implementation (Service-Coupled Logic)

Currently, this logic lives inside `InvitationService.cs`. It is mixed with database calls and HTTP logic.

```csharp
// Current: VendorMdm.Api/Services/InvitationService.cs

public async Task CreateInvitationAsync(CreateInvitationRequest request)
{
    // Logic 1: Sanctions Check (Hardcoded in Service)
    var screeningResult = await _sanctionsService.ScreenEntityAsync(request.VendorName);
    if (screeningResult.Risk == "High") 
    {
        throw new Exception("Blocked"); 
    }

    // Logic 2: Account Group Mapping (Implicit Logic)
    string accountGroup;
    if (request.VendorType == "EventParticipant")
    {
         accountGroup = "Z003"; // Arbitrary code hidden in service
    }
    else 
    {
         accountGroup = "Z001";
    }

    // ... Create SQL Entity ...
}
```

*   **Problem**: If we add a new flow (e.g., "Grant Application"), we have to copy-paste this logic or write new `if` statements.

---

## 3. Ontology Implementation (The New Layer)

We create a **Logical Layer** that defines these rules on the *Concept* itself.

### A. The Concept Definition
`backend/VendorMdm.Shared/Ontology/Concepts/IVendorConcept.cs`

```csharp
public interface IVendorConcept
{
    // The Core Data (State)
    string LegalName { get; }
    string? TaxId { get; }
    RiskLevel SanctionsRisk { get; }
    
    // The Origin (Where did they come from?)
    VendorOrigin Origin { get; }

    // Domain Logic (The Rules)
    bool IsEligibleForInvitation(out string reason);
    string DetermineAccountGroup();
}

public enum VendorOrigin { Direct, Event, Grant }
```

### B. The Concrete Implementation
`backend/VendorMdm.Shared/Ontology/Concepts/Vendor.cs`

```csharp
public class Vendor : IVendorConcept
{
    public string LegalName { get; init; }
    public string? TaxId { get; init; }
    public RiskLevel SanctionsRisk { get; init; }
    public VendorOrigin Origin { get; init; }

    public bool IsEligibleForInvitation(out string reason)
    {
        // Rule: Universal Sanctions Block
        if (SanctionsRisk == RiskLevel.High) 
        {
            reason = "Sanctions Risk is High";
            return false;
        }

        // Rule: Direct Invites require Tax ID
        if (Origin == VendorOrigin.Direct && string.IsNullOrEmpty(TaxId))
        {
             reason = "Direct vendors must have Tax ID";
             return false;
        }

        reason = "OK";
        return true;
    }

    public string DetermineAccountGroup()
    {
        return Origin switch 
        {
            VendorOrigin.Event => "Z003", // Event Vendor
            VendorOrigin.Grant => "Z005", // Grant Recipient
            _ => "Z001"                   // Standard Trade
        };
    }
}
```

---

## 4. The "Usage" (Refactored Service)

The Service now delegates logic to the Ontology. It becomes a simple coordinator.

`backend/VendorMdm.Api/Services/InvitationService.cs` (Refactored)

```csharp
public async Task CreateInvitationAsync(CreateInvitationRequest request)
{
    // 1. Hydrate the Concept (Factory)
    var vendorConcept = new Vendor 
    { 
        LegalName = request.LegalName,
        SanctionsRisk = await _sanctionsService.GetRiskAsync(request.LegalName),
        Origin = request.IsEvent ? VendorOrigin.Event : VendorOrigin.Direct
    };

    // 2. Ask the Concept: "Are you allowed?"
    if (!vendorConcept.IsEligibleForInvitation(out var failReason))
    {
        throw new DomainRuleException(failReason);
    }

    // 3. Ask the Concept: "What is your Account Group?"
    var derivedAccountGroup = vendorConcept.DetermineAccountGroup();

    // 4. Persistence (Save to SQL as usual)
    var sqlEntity = new VendorInvitation 
    {
        AccountGroup = derivedAccountGroup, // Logic came from Ontology
        // ...
    };
}
```

## Summary of Value
1.  **Centralized Rules**: The logic for "Account Group" and "Eligibility" is in `Vendor.cs`, not buried in Services.
2.  **Testability**: We can unit test `Vendor.IsEligibleForInvitation()` without mocking a Database or Sanctions Service.
3.  **Clarity**: The `InvitationService` reads like a story ("Hydrate Concept -> Check Rules -> Save"), not a script.
