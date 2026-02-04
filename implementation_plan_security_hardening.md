# Implementation Plan: Security Headers & Input Sanitization

**Date**: 2026-02-03
**Spec**: [specs/spec_security_headers_middleware.md](specs/spec_security_headers_middleware.md)
**Branch**: `feature/security-hardening`
**Verification Script**: [scripts/verification/verify_security_headers.sh](scripts/verification/verify_security_headers.sh)

---

## Overview

This plan implements CRITICAL security hardening to address ZERO TOLERANCE violations:
1. Security Headers Middleware (HSTS, CSP, X-Frame-Options, etc.)
2. Environment-Based CORS Configuration
3. Input Sanitization via IInputSanitizer

**Estimated Time**: 4-6 hours
**Risk Level**: LOW (additive changes, no breaking changes)

---

## Implementation Steps

### Phase 1: Core.Framework - IInputSanitizer (30 minutes)

**Goal**: Add input sanitization capability to Core.Framework

**Files to Create**:
1. `backend/VendorMdm.Core.Framework/Security/IInputSanitizer.cs`
2. `backend/VendorMdm.Core.Framework/Security/InputSanitizer.cs`

**Files to Modify**:
1. `backend/VendorMdm.Core.Framework/Extensions/ServiceCollectionExtensions.cs`
   - Add: `services.AddSingleton<IInputSanitizer, InputSanitizer>();`

**Verification**:
```bash
cd backend/VendorMdm.Core.Framework
dotnet build --configuration Release
# Expected: Build succeeded, 0 Error(s)
```

---

### Phase 2: SecurityHeadersMiddleware (45 minutes)

**Goal**: Create middleware to inject security headers on every response

**Files to Create**:
1. `backend/VendorMdm.Api/Middleware/SecurityHeadersMiddleware.cs`

**Files to Modify**:
1. `backend/VendorMdm.Api/Program.cs`
   - Add middleware registration BEFORE `app.UseAuthentication()`
   - Add: `app.UseMiddleware<SecurityHeadersMiddleware>();`

**Implementation Details**:
- Inject headers: HSTS, CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, X-XSS-Protection
- Generate CSP nonces for inline scripts
- Environment-aware (HSTS only on HTTPS/Production)
- Log header injection for audit

---

### Phase 3: CORS Configuration (30 minutes)

**Goal**: Replace hardcoded CORS origins with environment-based configuration

**Files to Modify**:
1. `backend/VendorMdm.Api/Program.cs`
   - Create `GetAllowedOrigins(IConfiguration)` helper function
   - Update CORS policy to use dynamic origins

2. `backend/VendorMdm.Api/appsettings.json`
   - Add `App:BaseUrl` configuration

---

### Phase 4: InputSanitizationActionFilter (45 minutes)

**Goal**: Automatically sanitize all DTO inputs before controller actions

**Files to Create**:
1. `backend/VendorMdm.Api/Filters/InputSanitizationActionFilter.cs`

**Files to Modify**:
1. `backend/VendorMdm.Api/Program.cs`
   - Register filter globally

---

### Phase 5: Testing & Verification (60 minutes)

**Steps**:
1. Build verification
2. Run verification script
3. Manual security header check
4. CORS environment test
5. Input sanitization test
6. Regression testing

---

### Phase 6: Pre-Commit Checks (15 minutes)

**Steps** (From moderngoldenrules.md Section 8):
1. Backend build (Release)
2. Frontend build
3. Migration size check
4. Alignment verification
5. Git status review

---

### Phase 7: Commit & Documentation (15 minutes)

**Steps**:
1. Stage changes
2. Conventional commit
3. Push to remote

---

## Success Criteria

- [ ] SecurityHeadersMiddleware implemented and registered
- [ ] 6 security headers present
- [ ] CORS configuration is environment-based
- [ ] Localhost blocked in Production CORS
- [ ] IInputSanitizer implemented in Core.Framework
- [ ] InputSanitizationActionFilter applied globally
- [ ] Verification script passes 100%
- [ ] Build succeeds: 0 errors, 0 warnings (Release)
- [ ] Pre-commit checks passed

---

**Status**: Ready for Implementation
**Next Step**: Begin Phase 1 (IInputSanitizer implementation)
