# Result Pattern Standard

**Category**: Core Development
**Pattern #**: 5
**Status**: MANDATORY

---

## Definition

ALL service methods MUST return `Result<T>` or `Result` instead of throwing exceptions for business logic failures.

---

## Rules

1. **NEVER** throw exceptions for expected business failures
2. **ALWAYS** return `Result.Failure("message")` for business errors
3. **ALWAYS** return `Result<T>.Success(value)` for success
4. **ONLY** throw exceptions for unexpected system failures

---

## Implementation

### Result Types (Core.Framework)

```csharp
// Success with value
public static Result<T> Success(T value) => new(value, true, null);

// Success without value
public static Result Success() => new(true, null);

// Failure
public static Result Failure(string error) => new(false, error);
public static Result<T> Failure<T>(string error) => new(default, false, error);
```

### Service Method Pattern

```csharp
// ✅ CORRECT: Return Result
public async Task<Result<Vendor>> GetVendorAsync(Guid id)
{
    var vendor = await _context.Vendors.FindAsync(id);
    if (vendor == null)
        return Result<Vendor>.Failure("Vendor not found");

    return Result<Vendor>.Success(vendor);
}

// ❌ FORBIDDEN: Throw exception for business logic
public async Task<Vendor> GetVendorAsync(Guid id)
{
    var vendor = await _context.Vendors.FindAsync(id);
    if (vendor == null)
        throw new NotFoundException("Vendor not found"); // ❌ NO!

    return vendor;
}
```

### Controller Pattern

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetVendor(Guid id)
{
    var result = await _vendorService.GetVendorAsync(id);

    if (!result.IsSuccess)
        return NotFound(result.Error);

    return Ok(result.Value.ToDto());
}
```

---

## Anti-Patterns

❌ Throwing `NotFoundException`, `ValidationException` for business logic
❌ Using exceptions for flow control
❌ Returning null instead of Result.Failure
❌ Not checking `IsSuccess` before accessing `Value`

---

## Reference

- **Implementation**: `Core.Framework/Common/Result.cs`
- **Error Handling**: [error-handling-standard.md](error-handling-standard.md)
- **Golden Rules**: Section 4, Category 2 (Core Development)
