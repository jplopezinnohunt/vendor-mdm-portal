# Build Hygiene Standard

**Category**: Governance & Process
**Section**: 5
**Status**: MANDATORY

---

## Definition

Build processes MUST be clean and reproducible. Interface changes MUST update all implementations atomically.

---

## Rules

1. **CLEAN SWEEP**: Before builds or migrations, kill stale processes and clean artifacts
2. **INTERFACE INTEGRITY**: When changing an interface, update ALL implementations in one atomic turn
3. **DUPLICATE TYPE CHECK**: Before creating new classes, search for existing definitions
4. **HYGIENE**: Pinned dependencies, `no-any` TypeScript, mandatory verification scripts
5. **OBSERVABILITY**: `traceparent` propagation + `TraceId` UI overlays
6. **SIMULATION**: `[SIMULATION MODE]` logs for all external mocks

---

## Implementation

### Clean Sweep Protocol

```bash
# Before builds or migrations
pkill -f dotnet || true

# Clean artifacts
rm -rf backend/*/bin backend/*/obj
rm -rf frontend/node_modules/.cache

# Fresh build
dotnet build --configuration Release
npm run build
```

### Interface Integrity Protocol

When modifying an interface, update ALL implementations:

```csharp
// If you change this interface:
public interface IEmailService
{
    Task<Result> SendAsync(EmailMessage message);
    Task<Result> SendTemplatedAsync(string template, object model, string to);  // NEW
}

// You MUST update ALL implementations in the SAME commit:
// 1. AzureEmailService.cs
// 2. SimulatedEmailService.cs
// 3. MockEmailService.cs (tests)
```

### Duplicate Type Check

```bash
# BEFORE creating a new class, ALWAYS search:
grep -r "class TypeName\|static class TypeName" backend/

# BEFORE creating a new constant class:
grep -r "static class.*Constants\|StatusCode\|ErrorCode" backend/

# Example: Before creating DocumentStatus
grep -r "class DocumentStatus\|enum DocumentStatus" backend/
# If found → use existing, don't create duplicate
```

**Rationale**: Prevents CS0101 duplicate type errors (learned from DocumentStatus incident).

### Pinned Dependencies

```json
// package.json - Use exact versions
{
  "dependencies": {
    "react": "18.2.0",      // ✅ Exact
    "axios": "^1.6.0"       // ❌ Avoid caret
  }
}
```

```xml
<!-- .csproj - Use exact versions -->
<PackageReference Include="Polly" Version="8.2.0" />  <!-- ✅ Exact -->
```

### TypeScript No-Any

```typescript
// tsconfig.json
{
  "compilerOptions": {
    "noImplicitAny": true,
    "strict": true
  }
}

// ❌ FORBIDDEN
function process(data: any) { ... }

// ✅ CORRECT
function process(data: VendorDto) { ... }
```

### Observability Requirements

```csharp
// Traceparent propagation
public class TracingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString()
            ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Trace-Id"] = traceId;
        await _next(context);
    }
}
```

```typescript
// TraceId UI overlay (dev mode)
{process.env.NODE_ENV === 'development' && (
  <div className="trace-overlay">TraceId: {traceId}</div>
)}
```

### Simulation Mode Logging

```csharp
// All mocked external services MUST log with [SIMULATION MODE]
public class SimulatedSapService : ISapIntegrationService
{
    public Task<Result<string>> CreateVendorAsync(VendorPayload payload)
    {
        _logger.LogInformation("[SIMULATION MODE] SAP vendor created", new {
            vendorName = payload.Name,
            operation = "ZBAPI_VENDOR_CREATE"
        });
        return Task.FromResult(Result<string>.Success("SAP-SIM-001"));
    }
}
```

---

## Pre-Build Checklist

```bash
# 1. Kill stale processes
pkill -f dotnet || true

# 2. Check for duplicate types (if creating new class)
grep -r "class NewClassName" backend/

# 3. Clean build
rm -rf backend/*/bin backend/*/obj
dotnet build --configuration Release

# 4. Verify interface integrity
# If interface changed, ensure all implementations updated
```

---

## Anti-Patterns

❌ Exit Code 143/134 from stale dotnet processes
❌ Changing interface without updating all implementations
❌ Creating duplicate type definitions
❌ Using `any` in TypeScript
❌ Missing `[SIMULATION MODE]` in mock services
❌ Unpinned dependency versions

---

## Reference

- **Golden Rules**: Section 5
- **CI/CD**: [cicd-setup-standards.md](cicd-setup-standards.md)
- **Pre-Commit**: [pre-commit-standard.md](pre-commit-standard.md)
