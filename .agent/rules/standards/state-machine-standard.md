# State Machine Standard

**Category**: Core Development
**Pattern #**: 7
**Status**: MANDATORY

---

## Definition

All workflow entities MUST have explicit state transitions defined in Status Constants with validation.

---

## Rules

1. **ALWAYS** define valid transitions in Status Constants
2. **ALWAYS** validate transitions before changing state
3. **NEVER** allow arbitrary status changes
4. **ALWAYS** emit domain events on state transitions

---

## Implementation

### Status Constants with State Machine

```csharp
// Shared/Constants/VendorStatus.cs
public static class VendorStatus
{
    // States
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string UnderReview = "UnderReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Integrated = "Integrated";

    // Valid transitions
    public static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        { Draft, new[] { Submitted } },
        { Submitted, new[] { UnderReview, Rejected } },
        { UnderReview, new[] { Approved, Rejected } },
        { Approved, new[] { Integrated } },
        { Rejected, new[] { Draft } },
        { Integrated, Array.Empty<string>() }  // Terminal state
    };

    public static bool CanTransition(string from, string to)
    {
        return ValidTransitions.TryGetValue(from, out var allowed)
            && allowed.Contains(to);
    }
}
```

### Concept Implementation

```csharp
public class VendorConcept : IOntologyConcept
{
    public Result<Vendor> TransitionStatus(Vendor vendor, string newStatus, string userId)
    {
        if (!VendorStatus.CanTransition(vendor.Status, newStatus))
        {
            return Result<Vendor>.Failure(
                $"Cannot transition from {vendor.Status} to {newStatus}");
        }

        var oldStatus = vendor.Status;
        vendor.Status = newStatus;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.UpdatedBy = userId;

        // Emit domain event
        _eventDispatcher.Dispatch(new VendorStatusChangedEvent(
            vendor.Id, oldStatus, newStatus, userId));

        return Result<Vendor>.Success(vendor);
    }
}
```

### Visual State Machine

```
    ┌─────────┐
    │  Draft  │
    └────┬────┘
         │ Submit
         ▼
    ┌───────────┐
    │ Submitted │
    └─────┬─────┘
          │ Review
          ▼
   ┌─────────────┐      ┌──────────┐
   │ UnderReview │─────►│ Rejected │
   └──────┬──────┘      └────┬─────┘
          │ Approve          │ Revise
          ▼                  ▼
    ┌──────────┐        ┌─────────┐
    │ Approved │        │  Draft  │
    └────┬─────┘        └─────────┘
         │ Integrate
         ▼
   ┌────────────┐
   │ Integrated │ (Terminal)
   └────────────┘
```

---

## Anti-Patterns

❌ Direct status assignment without validation
❌ Missing transitions in ValidTransitions
❌ Not emitting events on state change
❌ Allowing transition to any status

---

## Reference

- **Examples**: `Shared/Constants/*Status.cs`
- **Golden Rules**: Section 10.2 Pattern 7
