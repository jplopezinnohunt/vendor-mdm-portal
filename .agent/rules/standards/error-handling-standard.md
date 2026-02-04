# Error Handling Standard

**Category**: Core Development
**Pattern #**: 10
**Status**: MANDATORY
**Priority**: 🔴 CRITICAL

---

## Definition

Consistent error handling across all layers. Business failures use Result pattern; only system failures throw exceptions.

---

## Error Classification

| Type | Handling | Example |
|------|----------|---------|
| **Business Error** | `Result.Failure()` | Vendor not found, Invalid status transition |
| **Validation Error** | `Result.Failure()` | Invalid email format, Missing required field |
| **System Error** | `throw Exception` | Database connection lost, External API timeout |
| **Security Error** | `throw + Log` | Unauthorized access, Token expired |

---

## Layer-Specific Handling

### 1. Domain/Concept Layer

```csharp
// ✅ CORRECT: Return Result for business logic
public Result<Vendor> TransitionStatus(string newStatus)
{
    if (!IsValidTransition(Status, newStatus))
        return Result<Vendor>.Failure($"Cannot transition from {Status} to {newStatus}");

    Status = newStatus;
    RaiseEvent(new VendorStatusChangedEvent(Id, Status, newStatus));
    return Result<Vendor>.Success(this);
}

// ❌ FORBIDDEN: Throw exception for business logic
public void TransitionStatus(string newStatus)
{
    if (!IsValidTransition(Status, newStatus))
        throw new InvalidOperationException("Invalid transition"); // NO!
}
```

### 2. Service Layer

```csharp
// ✅ CORRECT: Propagate Result, wrap system errors
public async Task<Result<VendorDto>> GetVendorAsync(Guid id)
{
    try
    {
        var vendor = await _repository.GetByIdAsync(id);
        if (vendor == null)
            return Result<VendorDto>.Failure("Vendor not found");

        return Result<VendorDto>.Success(vendor.ToDto());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get vendor", new { vendorId = id });
        throw; // Re-throw system errors
    }
}
```

### 3. Controller Layer

```csharp
// ✅ CORRECT: Map Result to HTTP status
[HttpGet("{id}")]
public async Task<IActionResult> GetVendor(Guid id)
{
    var result = await _vendorService.GetVendorAsync(id);

    if (!result.IsSuccess)
    {
        return result.Error switch
        {
            var e when e.Contains("not found") => NotFound(new ErrorResponse(e)),
            var e when e.Contains("unauthorized") => Forbid(),
            _ => BadRequest(new ErrorResponse(result.Error))
        };
    }

    return Ok(result.Value);
}
```

---

## Error Response Format

### Standard Error Response

```json
{
  "error": {
    "code": "VENDOR_NOT_FOUND",
    "message": "The requested vendor does not exist",
    "details": null,
    "traceId": "00-abc123-def456-00"
  }
}
```

### Validation Error Response

```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "One or more validation errors occurred",
    "details": [
      { "field": "email", "message": "Invalid email format" },
      { "field": "taxId", "message": "Tax ID is required" }
    ],
    "traceId": "00-abc123-def456-00"
  }
}
```

### Error Response Class

```csharp
public class ErrorResponse
{
    public ErrorData Error { get; set; }

    public ErrorResponse(string message, string code = null)
    {
        Error = new ErrorData
        {
            Code = code ?? "ERROR",
            Message = message,
            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
    }
}

public class ErrorData
{
    public string Code { get; set; }
    public string Message { get; set; }
    public object Details { get; set; }
    public string TraceId { get; set; }
}
```

---

## Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `VALIDATION_FAILED` | 400 | Input validation failed |
| `INVALID_OPERATION` | 400 | Business rule violation |
| `NOT_FOUND` | 404 | Resource doesn't exist |
| `CONFLICT` | 409 | Resource state conflict |
| `UNAUTHORIZED` | 401 | Authentication required |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `RATE_LIMITED` | 429 | Too many requests |
| `INTERNAL_ERROR` | 500 | Unexpected system error |
| `SERVICE_UNAVAILABLE` | 503 | Dependency unavailable |

---

## Global Exception Handler

```csharp
// Program.cs
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(exception, "Unhandled exception", new
        {
            path = context.Request.Path,
            method = context.Request.Method,
            traceId = Activity.Current?.Id
        });

        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => 401,
            KeyNotFoundException => 404,
            InvalidOperationException => 400,
            _ => 500
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ErrorResponse(
            message: context.Response.StatusCode == 500
                ? "An unexpected error occurred"
                : exception?.Message,
            code: "INTERNAL_ERROR"
        ));
    });
});
```

---

## Exception Hierarchy (System Errors Only)

```csharp
// Only for SYSTEM errors, not business logic
public class VendorMdmException : Exception
{
    public string ErrorCode { get; }

    public VendorMdmException(string message, string errorCode = "INTERNAL_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class ExternalServiceException : VendorMdmException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message)
        : base(message, "SERVICE_UNAVAILABLE")
    {
        ServiceName = serviceName;
    }
}
```

---

## Logging Errors

```csharp
// ✅ CORRECT: Structured error logging
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed", new
    {
        operation = "CreateVendor",
        vendorId = request.Id,
        errorType = ex.GetType().Name
    });
    throw;
}

// ❌ FORBIDDEN: String interpolation
catch (Exception ex)
{
    _logger.LogError($"Failed to create vendor {request.Id}: {ex.Message}");
}
```

---

## Anti-Patterns

❌ Throwing exceptions for business logic failures
❌ Catching and swallowing exceptions silently
❌ Generic error messages hiding real issues
❌ Exposing stack traces to clients
❌ Not logging exceptions before re-throwing
❌ Using exceptions for flow control
❌ Returning null instead of Result.Failure

---

## Decision Tree

```
Error Occurred
    │
    ├── Is it a business rule violation?
    │   └── YES → Return Result.Failure("message")
    │
    ├── Is it invalid user input?
    │   └── YES → Return Result.Failure("message")
    │
    ├── Is it a system/infrastructure failure?
    │   └── YES → Log + throw Exception
    │
    └── Is it a security violation?
        └── YES → Log as Warning + throw/return 401/403
```

---

## Reference

- **Result Pattern**: [result-pattern-standard.md](result-pattern-standard.md)
- **Logging**: [logging-standard.md](logging-standard.md)
- **Security**: [security-architecture.md](security-architecture.md)
- **Golden Rules**: Section 4, Category 2
