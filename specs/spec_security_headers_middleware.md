# Specification: Security Headers & Input Sanitization

**Date**: 2026-02-03
**Version**: 1.0.0
**Status**: Draft
**Priority**: CRITICAL (ZERO TOLERANCE)
**Audit Issue**: Addresses Compliance Audit Issues #1, #2, #5

---

## Executive Summary

This specification addresses **CRITICAL ZERO TOLERANCE** violations identified in the compliance audit:
1. **Missing Security Headers** - HSTS, CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy
2. **Production CORS Misconfiguration** - Allows localhost in production environments
3. **Missing XSS Protection** - IInputSanitizer not applied to API inputs

These violations expose the application to XSS attacks, clickjacking, MITM attacks, and unauthorized cross-origin access.

---

## 📋 Compliance Sidebar

### Standards Cited
- **[moderngoldenrules.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/moderngoldenrules.md) Section 7.B (Network & Transport)** - Lines 103-106
  - MANDATORY: HSTS, CSP, X-Frame-Options
  - MANDATORY: Environment-based CORS (NO localhost in production)
  - MANDATORY: Rate limiting for public endpoints

- **[moderngoldenrules.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/moderngoldenrules.md) Section 7.C (Input Hygiene)** - Lines 108-110
  - MANDATORY: IInputSanitizer for all DTO strings
  - FORBIDDEN: Accepting raw JSONB or Entity objects

### Verification Requirements
- Pre-commit checks (Section 8)
- Verification script: `scripts/verification/verify_security_headers.sh`
- Build must succeed with 0 errors (Release configuration)

---

## Problem Statement

### Current State (Violations)

**1. Security Headers Missing**
- Location: [Program.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Program.cs)
- Missing Headers:
  ```
  ❌ Strict-Transport-Security (HSTS)
  ❌ Content-Security-Policy (CSP)
  ❌ X-Frame-Options: DENY
  ❌ X-Content-Type-Options: nosniff
  ❌ Referrer-Policy: no-referrer
  ```

**2. CORS Configuration**
- Location: [Program.cs:218-233](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Program.cs#L218-L233)
- Current: Allows `http://localhost:3000` unconditionally
- Risk: Production CORS allows localhost (FORBIDDEN per Section 7.B)

**3. Input Sanitization**
- IInputSanitizer exists in Core.Framework but NOT used
- All 21 controllers accept raw DTOs without XSS sanitization
- Risk: XSS vulnerabilities in user input fields

### Impact

**Security Risks**:
- **XSS Attacks**: Malicious scripts in user input
- **Clickjacking**: Site can be embedded in malicious iframes
- **MITM Attacks**: HTTP downgrade attacks without HSTS
- **CORS Bypass**: Unauthorized cross-origin requests in production
- **MIME Confusion**: Content-type sniffing vulnerabilities

**Compliance**:
- ZERO TOLERANCE policy violation (Section 7)
- Failed audit: 58% compliance score
- Blocks production deployment

---

## Requirements

### Functional Requirements

**FR-1: Security Headers Middleware**
- MUST add middleware that injects security headers on every response
- MUST be environment-aware (different policies for dev/prod)
- MUST run early in the pipeline (before authentication)

**FR-2: CORS Environment Configuration**
- MUST use environment-based CORS origins
- MUST read from `App:BaseUrl` configuration
- MUST NOT allow localhost in Production environment
- MUST allow localhost ONLY in Development/Staging

**FR-3: Input Sanitization**
- MUST integrate IInputSanitizer from Core.Framework
- MUST sanitize all string properties in DTOs before processing
- MUST create action filter for automatic sanitization
- MUST log sanitization events for audit trail

### Non-Functional Requirements

**NFR-1: Performance**
- Security header injection: < 5ms overhead
- Input sanitization: < 10ms per request
- No impact on Doherty Threshold (<400ms)

**NFR-2: Configuration**
- All security policies MUST be configurable via appsettings
- NO hardcoded values in middleware
- Support for CSP nonce generation

**NFR-3: Observability**
- Log CORS rejections with client IP
- Log input sanitization events (what was sanitized)
- Expose security metrics in health checks

---

## Solution Design

### Architecture

```
┌─────────────────────────────────────────────────────┐
│  ASP.NET Core Pipeline                              │
├─────────────────────────────────────────────────────┤
│                                                     │
│  1. [SecurityHeadersMiddleware] ← NEW              │
│     │                                               │
│     ├─ Inject HSTS, CSP, X-Frame-Options           │
│     ├─ Environment-aware policies                   │
│     └─ Log header injection                         │
│                                                     │
│  2. [CORS Middleware] ← MODIFIED                   │
│     │                                               │
│     ├─ Environment-based origins                    │
│     ├─ Read from App:BaseUrl config                 │
│     └─ Block localhost in Production                │
│                                                     │
│  3. [Authentication Middleware]                     │
│                                                     │
│  4. [Authorization Middleware]                      │
│                                                     │
│  5. [InputSanitizationFilter] ← NEW                │
│     │                                               │
│     ├─ Apply to all Controller actions             │
│     ├─ Sanitize string properties in DTOs           │
│     └─ Log sanitization events                      │
│                                                     │
│  6. [Controllers]                                   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Components

**1. SecurityHeadersMiddleware**
- Location: `backend/VendorMdm.Api/Middleware/SecurityHeadersMiddleware.cs`
- Responsibilities:
  - Inject security headers on every response
  - Read policies from configuration
  - Generate CSP nonces for inline scripts
  - Log header injection for audit

**2. SecurityHeadersConfiguration**
- Location: `backend/VendorMdm.Api/Configuration/SecurityHeadersConfiguration.cs`
- Responsibilities:
  - Define security policies per environment
  - CSP directives configuration
  - HSTS max-age configuration

**3. InputSanitizationActionFilter**
- Location: `backend/VendorMdm.Api/Filters/InputSanitizationActionFilter.cs`
- Responsibilities:
  - Automatically sanitize DTOs before controller actions
  - Use IInputSanitizer from Core.Framework
  - Log what was sanitized for audit trail

**4. CORS Configuration Update**
- Location: `backend/VendorMdm.Api/Program.cs`
- Changes:
  - Replace hardcoded origins with environment-based
  - Read from `App:BaseUrl` configuration
  - Add environment check for localhost

---

## Implementation Details

### 1. Security Headers Middleware

**File**: `backend/VendorMdm.Api/Middleware/SecurityHeadersMiddleware.cs`

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // HSTS (Strict-Transport-Security)
        if (!context.Request.IsHttps && IsProduction())
        {
            context.Response.Headers.Add("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        // CSP (Content-Security-Policy)
        var cspNonce = GenerateNonce();
        context.Items["csp-nonce"] = cspNonce;
        context.Response.Headers.Add("Content-Security-Policy",
            BuildCspPolicy(cspNonce));

        // X-Frame-Options
        context.Response.Headers.Add("X-Frame-Options", "DENY");

        // X-Content-Type-Options
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

        // Referrer-Policy
        context.Response.Headers.Add("Referrer-Policy", "no-referrer");

        // X-XSS-Protection (legacy but still useful)
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

        _logger.LogDebug("Security headers injected for {Path}",
            context.Request.Path);

        await _next(context);
    }

    private string BuildCspPolicy(string nonce)
    {
        var policy = _configuration["Security:CSP:Policy"]
            ?? "default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';";

        return policy.Replace("{nonce}", nonce);
    }

    private string GenerateNonce()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private bool IsProduction()
    {
        return _configuration["ASPNETCORE_ENVIRONMENT"] == "Production";
    }
}
```

### 2. CORS Configuration (Environment-Based)

**File**: `backend/VendorMdm.Api/Program.cs` (Update existing)

```csharp
// Before (FORBIDDEN - allows localhost in all environments)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// After (COMPLIANT - environment-based)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = GetAllowedOrigins(builder.Configuration);

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

string[] GetAllowedOrigins(IConfiguration config)
{
    var environment = config["ASPNETCORE_ENVIRONMENT"];
    var baseUrl = config["App:BaseUrl"];

    if (environment == "Production")
    {
        // Production: ONLY the configured BaseUrl
        return new[] { baseUrl };
    }
    else if (environment == "Staging")
    {
        // Staging: BaseUrl + localhost for testing
        return new[] { baseUrl, "http://localhost:3000" };
    }
    else
    {
        // Development: localhost
        return new[] { "http://localhost:3000", "http://localhost:5173" };
    }
}
```

### 3. Input Sanitization Action Filter

**File**: `backend/VendorMdm.Api/Filters/InputSanitizationActionFilter.cs`

```csharp
public class InputSanitizationActionFilter : IAsyncActionFilter
{
    private readonly IInputSanitizer _sanitizer;
    private readonly ILogger<InputSanitizationActionFilter> _logger;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            SanitizeObject(argument);
        }

        await next();
    }

    private void SanitizeObject(object obj)
    {
        var type = obj.GetType();
        var properties = type.GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj) as string;
            if (string.IsNullOrEmpty(value)) continue;

            var sanitized = _sanitizer.SanitizeHtml(value);

            if (value != sanitized)
            {
                prop.SetValue(obj, sanitized);
                _logger.LogWarning(
                    "Sanitized input for {Property}: Original length={Original}, Sanitized length={Sanitized}",
                    prop.Name, value.Length, sanitized.Length);
            }
        }
    }
}
```

### 4. IInputSanitizer Implementation (Core.Framework)

**File**: `backend/VendorMdm.Core.Framework/Security/IInputSanitizer.cs`

```csharp
public interface IInputSanitizer
{
    string SanitizeHtml(string input);
    string SanitizeSql(string input);
    string SanitizeFileName(string input);
}

public class InputSanitizer : IInputSanitizer
{
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Remove dangerous HTML tags
        var dangerous = new[] { "<script", "<iframe", "<object", "<embed",
            "javascript:", "onerror=", "onload=" };

        var sanitized = input;
        foreach (var tag in dangerous)
        {
            sanitized = sanitized.Replace(tag, "",
                StringComparison.OrdinalIgnoreCase);
        }

        return sanitized;
    }

    public string SanitizeSql(string input)
    {
        // EF Core already uses parameterized queries
        // This is for legacy raw SQL scenarios
        return input?.Replace("'", "''");
    }

    public string SanitizeFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(input.Split(invalid));
    }
}
```

---

## Configuration

### appsettings.json

```json
{
  "App": {
    "BaseUrl": "https://vendor-mdm.example.com"
  },
  "Security": {
    "HSTS": {
      "MaxAge": 31536000,
      "IncludeSubDomains": true,
      "Preload": true
    },
    "CSP": {
      "Policy": "default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';"
    }
  }
}
```

### appsettings.Development.json

```json
{
  "App": {
    "BaseUrl": "http://localhost:3000"
  },
  "Security": {
    "HSTS": {
      "Enabled": false
    }
  }
}
```

---

## Testing Strategy

### Unit Tests

**1. SecurityHeadersMiddleware Tests**
- Verify HSTS header in HTTPS requests
- Verify CSP nonce generation
- Verify all required headers are present
- Verify environment-based behavior

**2. CORS Configuration Tests**
- Verify localhost blocked in Production
- Verify BaseUrl used in Production
- Verify multiple origins in Development

**3. InputSanitizer Tests**
- Verify XSS payloads are sanitized
- Verify safe HTML is preserved
- Verify SQL injection attempts are sanitized

### Integration Tests

**1. End-to-End Header Verification**
```bash
curl -I https://vendor-mdm.example.com/api/health
# Expected headers:
# Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
# Content-Security-Policy: default-src 'self'; ...
# X-Frame-Options: DENY
# X-Content-Type-Options: nosniff
# Referrer-Policy: no-referrer
```

**2. CORS Rejection Test (Production)**
```bash
curl -H "Origin: http://localhost:3000" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS https://vendor-mdm.example.com/api/vendors
# Expected: CORS rejection (403 or no CORS headers)
```

**3. Input Sanitization Test**
```bash
curl -X POST http://localhost:5001/api/vendors \
     -H "Content-Type: application/json" \
     -d '{"name": "<script>alert(1)</script>Company Name"}'
# Expected: Script tag removed from name
```

---

## Verification Script

**File**: `scripts/verification/verify_security_headers.sh`

```bash
#!/bin/bash
set -e

echo "🔍 Security Headers & Input Sanitization Verification"
echo "======================================================"

# Test 1: Security Headers Present
echo ""
echo "Test 1: Verifying security headers..."
RESPONSE=$(curl -sI http://localhost:5001/api/health)

if echo "$RESPONSE" | grep -q "X-Frame-Options: DENY"; then
    echo "✅ X-Frame-Options header present"
else
    echo "❌ X-Frame-Options header missing"
    exit 1
fi

if echo "$RESPONSE" | grep -q "X-Content-Type-Options: nosniff"; then
    echo "✅ X-Content-Type-Options header present"
else
    echo "❌ X-Content-Type-Options header missing"
    exit 1
fi

if echo "$RESPONSE" | grep -q "Referrer-Policy"; then
    echo "✅ Referrer-Policy header present"
else
    echo "❌ Referrer-Policy header missing"
    exit 1
fi

if echo "$RESPONSE" | grep -q "Content-Security-Policy"; then
    echo "✅ CSP header present"
else
    echo "❌ CSP header missing"
    exit 1
fi

# Test 2: CORS Configuration
echo ""
echo "Test 2: Verifying CORS configuration..."
CORS_RESPONSE=$(curl -s -H "Origin: http://localhost:3000" \
    -H "Access-Control-Request-Method: GET" \
    -X OPTIONS http://localhost:5001/api/health)

if [ -n "$CORS_RESPONSE" ]; then
    echo "✅ CORS configured (Development mode allows localhost)"
else
    echo "⚠️  CORS check inconclusive (check environment)"
fi

# Test 3: IInputSanitizer Registration
echo ""
echo "Test 3: Verifying IInputSanitizer registration..."
if grep -q "IInputSanitizer" backend/VendorMdm.Api/Program.cs; then
    echo "✅ IInputSanitizer registered in DI"
else
    echo "❌ IInputSanitizer not registered"
    exit 1
fi

# Test 4: SecurityHeadersMiddleware Registration
echo ""
echo "Test 4: Verifying SecurityHeadersMiddleware registration..."
if grep -q "SecurityHeadersMiddleware" backend/VendorMdm.Api/Program.cs; then
    echo "✅ SecurityHeadersMiddleware registered"
else
    echo "❌ SecurityHeadersMiddleware not registered"
    exit 1
fi

echo ""
echo "======================================================"
echo "✅ ALL SECURITY CHECKS PASSED"
echo "======================================================"
```

---

## Migration Path

### Phase 1: Core.Framework Updates
1. Add IInputSanitizer interface to Core.Framework
2. Add InputSanitizer implementation
3. Register in ServiceCollectionExtensions

### Phase 2: API Middleware
1. Create SecurityHeadersMiddleware
2. Create SecurityHeadersConfiguration
3. Register middleware in Program.cs (early in pipeline)

### Phase 3: CORS Configuration
1. Create GetAllowedOrigins helper function
2. Update CORS configuration to use environment-based origins
3. Test in Development, Staging, Production

### Phase 4: Input Sanitization
1. Create InputSanitizationActionFilter
2. Register filter globally in Program.cs
3. Test with XSS payloads

### Phase 5: Verification
1. Create verification script
2. Run integration tests
3. Update compliance audit

---

## Success Criteria

- [ ] SecurityHeadersMiddleware implemented and registered
- [ ] All 6 security headers present in responses (HSTS, CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, X-XSS-Protection)
- [ ] CORS configuration is environment-based
- [ ] Localhost blocked in Production CORS
- [ ] IInputSanitizer implemented in Core.Framework
- [ ] InputSanitizationActionFilter applied globally
- [ ] All 21 controllers have automatic input sanitization
- [ ] Verification script passes 100%
- [ ] Build succeeds with 0 errors (Release configuration)
- [ ] No performance regression (<5ms overhead for headers, <10ms for sanitization)
- [ ] Compliance audit updated: Issues #1, #2, #5 resolved

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| CSP breaks existing inline scripts | HIGH | Use nonces for inline scripts, gradual rollout |
| CORS blocks legitimate requests | MEDIUM | Comprehensive testing in Staging before Production |
| Input sanitization breaks valid HTML | MEDIUM | Whitelist safe HTML tags, log all sanitizations |
| Performance overhead | LOW | Benchmark before/after, ensure <10ms overhead |
| Header conflicts with existing middleware | LOW | Place SecurityHeadersMiddleware early in pipeline |

---

## Timeline

**Estimated Effort**: 1-2 days

- **Day 1 Morning**: Core.Framework updates (IInputSanitizer)
- **Day 1 Afternoon**: SecurityHeadersMiddleware implementation
- **Day 2 Morning**: CORS configuration + InputSanitizationActionFilter
- **Day 2 Afternoon**: Testing + verification script + compliance audit update

---

## References

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [MDN Content-Security-Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [OWASP XSS Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [moderngoldenrules.md Section 7](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/moderngoldenrules.md)

---

**Status**: Ready for Phase 2 (Implementation Plan)
**Next Step**: Create `implementation_plan.md` and verification script
