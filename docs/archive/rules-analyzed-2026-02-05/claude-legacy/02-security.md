# Security: The Iron Dome (ZERO TOLERANCE)

---

## 2.1 Eight Security Layers

| Layer | Component | Implementation |
|-------|-----------|----------------|
| 1 | **Authentication** | Azure AD, JWT validation |
| 2 | **Authorization** | App-scoped RBAC |
| 3 | **Network** | HSTS, CSP, X-Frame-Options |
| 4 | **Input** | XSS sanitization |
| 5 | **Rate Limiting** | 5 req/min/IP for anonymous |
| 6 | **Ghost User Blocking** | Block AD users not in DB |
| 7 | **Session** | 15-minute sliding window |
| 8 | **Secrets** | KeyVault (Prod), UserSecrets (Dev) |

---

## 2.2 Authentication & Session

### Azure AD Integration
```csharp
// Token validation
ClockSkew = TimeSpan.Zero  // Strict expiration
SessionTimeout = 15 minutes (sliding window)
```

### Secrets Management
- **Production**: Azure Key Vault
- **Development**: User Secrets
- **Rule**: ZERO hardcoded secrets in code

### Impersonation Security
- Cryptographically signed tokens
- Audit trail for all impersonation actions
- Disabled in Production

---

## 2.3 Authorization (RBAC)

### Core Roles
| Role | Description | Scope |
|------|-------------|-------|
| `SystemAdmin` | Full access | ALL apps |
| `AppAdmin` | Full access | ONE app |
| `Viewer` | Read-only | Per app |
| `Editor` | Create/edit | Per app |
| `Approver` | Approve/reject | Per app |
| `Auditor` | View audit logs | Per app |

### Authorization Check
```csharp
// ✅ CORRECT - App-scoped
var hasRole = await _authz.HasRoleAsync(userId, "Approver", "VendorMDM");

// ❌ WRONG - No app scope
var hasRole = User.IsInRole("Approver");
```

---

## 2.4 Network & Transport

### Security Headers (MANDATORY)
```csharp
app.UseHsts();  // Strict-Transport-Security
app.UseCsp();   // Content-Security-Policy
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Add("X-Frame-Options", "DENY");
    await next();
});
```

### CORS Policy
- **Development**: `localhost:3000`, `localhost:3002`
- **Production**: Specific `App:BaseUrl` only
- **Rule**: NO localhost in Production

---

## 2.5 Input Validation & Sanitization

### XSS Protection
```csharp
// Safe tags whitelist
var allowedTags = new[] { "p", "br", "strong", "em", "a", "ul", "ol", "li" };

// Sanitize all input
var sanitized = _sanitizer.Sanitize(userInput);
```

### DTO Enforcement
```csharp
// ✅ CORRECT
public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)

// ❌ WRONG - Raw entity
public async Task<IActionResult> Create([FromBody] Vendor vendor)
```

---

## 2.6 Rate Limiting

### Anonymous Endpoints
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("anonymous", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // Reject immediately
    });
});
```

### Protected Endpoints
- `AuthController`: Login, MFA, Magic Link
- `InvitationController`: Validate, Complete
- `SystemController`: Health check

---

## 2.7 Ghost User Blocking

### Middleware
```csharp
public class GhostUserBlockingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var userExists = await _context.Users.AnyAsync(u => u.Email == userEmail);

            if (!userExists && _environment.IsProduction())
            {
                context.Response.StatusCode = 403;
                return;
            }
        }
        await _next(context);
    }
}
```

---

## 2.8 Security Checklist

### Authentication ✅
- [x] Azure AD integration
- [x] JWT validation (issuer, audience, lifetime)
- [x] No hardcoded secrets
- [x] 15-minute session timeout
- [x] Signed impersonation tokens

### Authorization ✅
- [x] App-scoped RBAC
- [x] Claims transformation
- [x] Ghost user blocking (Production)

### Network ✅
- [x] HSTS headers
- [x] CSP headers
- [x] X-Frame-Options: DENY
- [x] Strict CORS (Production)
- [x] Rate limiting (anonymous endpoints)

### Input ✅
- [x] HTML sanitization
- [x] DTO enforcement
- [x] No raw JSONB from client
