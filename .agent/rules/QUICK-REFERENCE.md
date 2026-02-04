# Quick Reference Card (1-Page Cheat Sheet)

**Version**: 1.2.0 | **Standards**: 34 | **ADRs**: 5 | **Full Rules**: [moderngoldenrules.md](moderngoldenrules.md)

---

## 🔴 CRITICAL RULES (Never Break)

| Rule | Command/Pattern |
|------|-----------------|
| **Zero Data Loss** | NEVER `rm -rf` or delete `*.db` without explicit consent |
| **SDD Workflow** | Spec → Plan → Implement → Verify (no shortcuts) |
| **Branching** | Always `feature/*` from `develop`, never `main` |
| **No Hardcoded Secrets** | KeyVault (prod) / UserSecrets (dev) |
| **Result Pattern** | Return `Result<T>`, never throw for business errors |

---

## 🟠 BEFORE YOU CODE

```bash
# 1. Create spec
specs/spec_[feature].md  # with Compliance Sidebar

# 2. Create verification script
scripts/verification/verify_[feature].sh

# 3. Create branch
git checkout develop && git checkout -b feature/[name]
```

---

## 🟡 PATTERNS CHEAT SHEET

### ✅ DO

```csharp
// Result pattern
return Result<T>.Success(value);
return Result.Failure("error message");

// Structured logging
_logger.LogInformation("Event", new { data });

// Headers (safe)
context.Response.Headers["X-Frame-Options"] = "DENY";

// Environment check
if (env.EnvironmentName == "Staging") { }

// Soft delete
entity.IsDeleted = true;
entity.DeletedAt = DateTime.UtcNow;
```

### ❌ DON'T

```csharp
// Never throw for business logic
throw new NotFoundException("Not found");

// Never string interpolation in logs
_logger.LogInformation($"User {userId}");

// Never Headers.Add (throws on duplicate)
context.Response.Headers.Add("X-Frame", "DENY");

// Never IsStaging() (doesn't exist)
if (env.IsStaging()) { }

// Never hard delete
_context.Remove(entity);
```

---

## 📋 PRE-COMMIT CHECKLIST

```bash
# 1. Build backend
cd backend/VendorMdm.Api && dotnet build --configuration Release

# 2. Build frontend
cd frontend && npm run build

# 3. Run verification
./scripts/verification/verify_[feature].sh

# 4. Check warnings (0 critical allowed)
# Review build output for CS0618, CS8600, ASP0019

# 5. Git status (no secrets, no unintended files)
git status
```

---

## 🏗️ ARCHITECTURE LAYERS

```
Controller → Service → Concept → Repository → DbContext
     ↓           ↓          ↓
   DTO      Result<T>   Domain Event
```

**FORBIDDEN**: Business logic in Controllers/Services (use Concepts)

---

## 📊 PERFORMANCE TARGETS

| Metric | Target |
|--------|--------|
| UI Response | < 400ms (Doherty Threshold) |
| API Response | < 200ms (p95) |
| Input Sanitization | < 10ms |
| Build Time | < 60s |

---

## 🔒 SECURITY HEADERS (Mandatory)

```
Strict-Transport-Security: max-age=31536000
Content-Security-Policy: default-src 'self'
X-Frame-Options: DENY
X-Content-Type-Options: nosniff
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=()
```

---

## 📁 FILE STRUCTURE

```
specs/spec_*.md           → Specifications
scripts/verification/     → Verification scripts
.agent/rules/standards/   → 32 detailed standards
.agent/retrospectives/    → Learnings database
```

---

## 🆘 WHEN STUCK

1. Check [INDEX.md](../retrospectives/INDEX.md) for known issues
2. Read relevant standard in `standards/`
3. Search retrospectives for similar problems
4. Document solution in retrospective when found

---

**End of Quick Reference** | Full docs: [moderngoldenrules.md](moderngoldenrules.md)
