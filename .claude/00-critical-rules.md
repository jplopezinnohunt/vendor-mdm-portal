# Critical Rules - ZERO TOLERANCE

**Priority**: HIGHEST - Always check these rules first

---

## 0.1 ZERO DATA LOSS Policy

**FORBIDDEN Actions**:
- Delete, reset, or overwrite database files (`*.db`, `*.sqlite`)
- Recursive deletions (`rm -rf`) without EXPLICIT WRITTEN CONSENT
- Drop tables or databases without explicit approval

**Recovery Priority**:
- If migration fails → Fix the migration script
- NEVER delete database to "start fresh"
- Always assume local data is production-critical

---

## 0.2 Pre-Commit Verification Protocol

**MANDATORY** checks before every commit:

### 1. Build Verification
```bash
# Backend
dotnet build --configuration Release
# Expected: Build succeeded, 0 Error(s)

# Frontend
npm run build
# Expected: 0 errors
```

### 2. Migration Size Check
```bash
ls -lh backend/VendorMdm.Api/Migrations/*.cs | grep -v Designer | grep -v Snapshot
# All migrations MUST be < 50KB
```

### 3. Git Status Review
```bash
git status
# Review all changed files
# Verify no sensitive data (keys, passwords)
```

### 4. Warning Review
- Fix all critical warnings immediately
- Document acceptable warnings in commit message

---

## 0.3 Interface Integrity Rule

When changing an interface, update ALL implementations in one atomic turn:
- Mock implementation
- Real implementation
- Simulation implementation
- Test implementation

---

## 0.4 Forbidden Shortcuts

The agent MUST decline:
- Bypassing spec-driven development
- Committing without verification
- Modifying Core.Framework without ADR
- Skipping test updates when changing interfaces

---

## 0.5 Emergency Bypass

For production-down emergencies only:
```bash
EMERGENCY_MODE=true dotnet build
```

After emergency:
1. Create ADR documenting the change
2. Submit PR for review
3. Clean up technical debt
