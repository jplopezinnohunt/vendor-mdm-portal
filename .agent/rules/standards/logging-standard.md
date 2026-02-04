# Structured Logging Standard

**Category**: Core Development
**Pattern #**: 6
**Status**: MANDATORY

---

## Definition

Use `IStructuredLogger` for all logging with structured data, NEVER string interpolation.

---

## Rules

1. **ALWAYS** use structured logging: `logger.LogInformation("Message", new { data })`
2. **NEVER** use string interpolation: `logger.LogInformation($"User {userId}")`
3. **ALWAYS** log security events (auth, access, data changes)
4. **ALWAYS** mask PII in logs

---

## Implementation

### Basic Logging

```csharp
// ✅ CORRECT: Structured logging
_logger.LogInformation("Vendor created", new { vendorId, createdBy, timestamp });

// ❌ FORBIDDEN: String interpolation
_logger.LogInformation($"Vendor {vendorId} created by {createdBy}");
```

### Security Events (MANDATORY)

```csharp
// Authentication
_logger.LogInformation("User login", new { userId, authMethod, ipAddress });
_logger.LogWarning("Login failed", new { email, reason, ipAddress, attemptCount });

// Authorization
_logger.LogWarning("Access denied", new { userId, resource, action, requiredRole });

// Data access
_logger.LogInformation("Sensitive data accessed", new { userId, entityType, entityId });

// Soft delete
_logger.LogInformation("Entity soft deleted", new { entityType, entityId, deletedBy, reason });
```

### PII Masking

```csharp
// ✅ CORRECT: Mask PII
_logger.LogInformation("User registered", new {
    email = email.MaskEmail(),  // "joh***@example.com"
    phone = phone.MaskPhone()   // "***-***-1234"
});

// ❌ FORBIDDEN: Log full PII
_logger.LogInformation("User registered", new { email, phone });
```

### Extension Methods

```csharp
public static string MaskEmail(this string email)
{
    if (string.IsNullOrEmpty(email)) return "***";
    var parts = email.Split('@');
    return $"{parts[0].Substring(0, Math.Min(3, parts[0].Length))}***@{parts[1]}";
}
```

---

## Log Levels

| Level | Use Case |
|-------|----------|
| **Trace** | Detailed debugging (disabled in prod) |
| **Debug** | Development diagnostics |
| **Information** | Normal operations, audit events |
| **Warning** | Potential issues, failed auth |
| **Error** | Failures requiring attention |
| **Critical** | System failures |

---

## Reference

- **Implementation**: `Core.Framework/Logging/IStructuredLogger.cs`
- **Golden Rules**: Section 10.2 Pattern 5
