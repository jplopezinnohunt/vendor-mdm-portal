# GDPR & PII Standard

**Category**: Security & Compliance
**Pattern #**: 12-13
**Status**: MANDATORY

---

## Definition

Handle personally identifiable information (PII) with GDPR compliance including masking, anonymization, and consent tracking.

---

## Rules

### PII Masking (Pattern 12)

1. **NEVER** log full PII (emails, phones, SSN, credit cards)
2. **ALWAYS** mask sensitive data in logs and displays
3. **ALWAYS** encrypt PII at rest

### GDPR Compliance (Pattern 13)

1. **ALWAYS** track consent for data processing
2. **ALWAYS** support right to be forgotten (anonymization)
3. **ALWAYS** support data export capability

---

## PII Masking Implementation

### Masking Extensions

```csharp
public static class PiiMaskingExtensions
{
    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrEmpty(email)) return "***";
        var parts = email.Split('@');
        var masked = parts[0].Length > 3
            ? parts[0].Substring(0, 3) + "***"
            : "***";
        return $"{masked}@{parts[1]}";
    }

    public static string MaskPhone(this string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4) return "***";
        return $"***-***-{phone.Substring(phone.Length - 4)}";
    }

    public static string MaskCreditCard(this string cc)
    {
        if (string.IsNullOrEmpty(cc) || cc.Length < 4) return "****";
        return $"****-****-****-{cc.Substring(cc.Length - 4)}";
    }
}
```

### Logging with Masking

```csharp
_logger.LogInformation("User registered", new {
    email = user.Email.MaskEmail(),
    phone = user.Phone.MaskPhone()
});
```

---

## GDPR Implementation

### Consent Tracking

```csharp
public class UserConsent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ConsentType { get; set; }  // "Marketing", "Analytics", "DataProcessing"
    public bool IsGranted { get; set; }
    public DateTime ConsentDate { get; set; }
    public string? IpAddress { get; set; }
}
```

### Right to Be Forgotten (Anonymization)

```csharp
public async Task<Result> AnonymizeUserAsync(Guid userId, string reason)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null) return Result.Failure("User not found");

    // Anonymize PII
    user.Email = $"anonymized_{Guid.NewGuid()}@deleted.local";
    user.FullName = "DELETED USER";
    user.Phone = null;
    user.Address = null;

    // Mark as anonymized
    user.IsAnonymized = true;
    user.AnonymizedAt = DateTime.UtcNow;
    user.AnonymizedReason = reason;

    await _context.SaveChangesAsync();

    _logger.LogInformation("User anonymized (GDPR)", new {
        userId,
        reason,
        timestamp = DateTime.UtcNow
    });

    return Result.Success();
}
```

### Data Export (Portability)

```csharp
public async Task<Result<UserDataExport>> ExportUserDataAsync(Guid userId)
{
    var user = await _context.Users
        .Include(u => u.Vendors)
        .Include(u => u.AuditLogs)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) return Result<UserDataExport>.Failure("User not found");

    var export = new UserDataExport
    {
        PersonalData = new {
            user.Email,
            user.FullName,
            user.Phone,
            user.CreatedAt
        },
        Vendors = user.Vendors.Select(v => v.ToDto()),
        AuditHistory = user.AuditLogs.Select(a => new {
            a.Action,
            a.Timestamp,
            a.EntityType
        }),
        ExportedAt = DateTime.UtcNow
    };

    return Result<UserDataExport>.Success(export);
}
```

---

## Anti-Patterns

❌ Logging full email addresses, phone numbers
❌ Storing SSN or credit cards in plain text
❌ No consent tracking
❌ Hard deleting instead of anonymizing
❌ No data export capability

---

## Reference

- **Golden Rules**: Section 10.3 Patterns 12-13
