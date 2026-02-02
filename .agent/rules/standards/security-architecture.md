# Security Architecture - Zero-Trust Implementation

## Overview
This document describes the comprehensive security approach for the Vendor MDM Portal, implementing the "Iron Dome" Zero-Trust Security standard.

---

## Security Layers

### 1. Authentication & Session Management ✅

#### Azure AD Integration
- **Provider**: Microsoft Entra ID (Azure AD)
- **Protocol**: OAuth 2.0 / OpenID Connect
- **Token Type**: JWT Bearer tokens
- **Validation**: Issuer, Audience, Lifetime, Signing Key

#### Session Configuration
```csharp
ClockSkew = TimeSpan.Zero  // Strict token expiration
SessionTimeout = 15 minutes (sliding window)
```

#### Secrets Management
- **Production**: Azure Key Vault
- **Development**: User Secrets
- **Rule**: ZERO hardcoded secrets in code

---

### 2. Authorization & Access Control ✅

#### Role-Based Access Control (RBAC)
- **App-Scoped Roles**: Roles tied to specific applications
- **Claims Transformation**: Azure AD groups → Application roles
- **Enforcement**: `IUserContext.HasRoleForApp(appId, role)`

#### Impersonation Security
- **Signed Tokens**: Cryptographically signed impersonation cookies
- **Audit Trail**: All impersonation actions logged
- **Production Block**: Impersonation disabled in Production

---

### 3. Network & Transport Security ✅

#### Security Headers
```csharp
HSTS: Strict-Transport-Security (HTTPS enforcement)
CSP: Content-Security-Policy (XSS prevention)
X-Frame-Options: DENY (Clickjacking prevention)
```

#### CORS Policy
- **Development**: `http://localhost:3000`, `http://localhost:3002`
- **Production**: Specific `App:BaseUrl` only
- **Rule**: NO localhost in Production

---

### 4. Input Validation & Sanitization ✅

#### XSS Protection
**Implementation**: `HtmlInputSanitizer` (Ganss.XSS library)

**Safe Tags Whitelist**:
```csharp
p, br, strong, em, u, a, ul, ol, li, h1-h6
```

**Usage**: All DTO strings sanitized before domain layer

#### DTO Enforcement
- **Rule**: Never accept raw JSONB or Entity objects from client
- **Pattern**: API → DTO → Domain Concept → Entity

---

### 5. Rate Limiting ✅ (NEW)

#### Anonymous Endpoint Protection
**Policy**: Fixed window rate limiting
- **Limit**: 5 requests per minute per IP
- **Window**: 1 minute
- **Queue**: Disabled (reject immediately)
- **Response**: HTTP 429 (Too Many Requests)

#### Protected Endpoints
- `AuthController`: Login, MFA, Magic Link (7 endpoints)
- `InvitationController`: Validate, Complete (2 endpoints)
- `SystemController`: Health check (1 endpoint)
- `AuthDiscoveryController`: Discovery (1 endpoint)

**Implementation**:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("anonymous", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

### 6. Ghost User Blocking ✅ (NEW)

#### Production Security
**Problem**: Users authenticated via Azure AD but not in database

**Solution**: `GhostUserBlockingMiddleware`
- **Check**: After authentication, verify user exists in DB
- **Production**: Block access (HTTP 403)
- **Development**: Allow (for testing)

**Implementation**:
```csharp
if (context.User.Identity.IsAuthenticated)
{
    var userExists = await dbContext.Users.AnyAsync(u => u.Email == userEmail);
    if (!userExists && isProduction)
    {
        return 403 Forbidden;
    }
}
```

---

## Threat Model

### Threats Mitigated

| Threat | Mitigation | Layer |
|--------|------------|-------|
| **Brute Force Login** | Rate limiting (5 req/min) | Network |
| **XSS Injection** | Input sanitization | Input |
| **CSRF** | SameSite cookies + CORS | Network |
| **Clickjacking** | X-Frame-Options: DENY | Transport |
| **Session Hijacking** | Short-lived tokens (15min) | Session |
| **Unauthorized Access** | RBAC + Ghost user blocking | Authorization |
| **Man-in-the-Middle** | HTTPS + HSTS | Transport |
| **Token Replay** | ClockSkew = 0 (strict expiration) | Authentication |

---

## Security Checklist

### ✅ Authentication
- [x] Azure AD integration
- [x] JWT validation (issuer, audience, lifetime)
- [x] No hardcoded secrets
- [x] 15-minute session timeout
- [x] Signed impersonation tokens

### ✅ Authorization
- [x] App-scoped RBAC
- [x] Claims transformation
- [x] Ghost user blocking (Production)

### ✅ Network
- [x] HSTS headers
- [x] CSP headers
- [x] X-Frame-Options: DENY
- [x] Strict CORS (Production)
- [x] Rate limiting (anonymous endpoints)

### ✅ Input
- [x] HTML sanitization
- [x] DTO enforcement
- [x] No raw JSONB from client

---

## Compliance Status

**Zero-Trust Security**: **100%** ✅

All "Iron Dome" requirements satisfied:
1. ✅ No Hardcoded Secrets
2. ✅ Signed Impersonation
3. ✅ Configurable Session Lifetime
4. ✅ Ghost User Blocking
5. ✅ Strict Security Headers
6. ✅ CORS Strictness
7. ✅ Rate Limiting
8. ✅ Input Sanitization
9. ✅ DTO Enforcement

---

## Monitoring & Alerts

### Security Events Logged
- Failed login attempts
- Rate limit violations
- Ghost user blocks
- Impersonation actions
- Input sanitization triggers

### Recommended Alerts
- **Critical**: Ghost user blocks in Production
- **High**: Rate limit violations (>10/min from single IP)
- **Medium**: Failed login attempts (>5 from single user)

---

## Future Enhancements (Optional)

1. **Multi-Factor Authentication (MFA)**: Already implemented for email-based auth
2. **IP Whitelisting**: Restrict admin endpoints to specific IPs
3. **Anomaly Detection**: ML-based unusual behavior detection
4. **Security Scanning**: Automated vulnerability scanning in CI/CD

---

**Last Updated**: 2026-02-02
**Compliance**: 100% Zero-Trust Security
