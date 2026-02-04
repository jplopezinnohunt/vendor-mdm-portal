# Pre-Commit Verification Standard

**Category**: Governance & Process
**Section**: 8
**Status**: MANDATORY

---

## Definition

All commits MUST pass mandatory verification checks. No commit without successful build, migration validation, and alignment verification.

---

## Rules

1. **BUILD VERIFICATION**: Backend and frontend must build with 0 errors
2. **MIGRATION SIZE**: All migration files must be < 50KB
3. **ALIGNMENT VERIFICATION**: Run verification scripts
4. **GIT STATUS REVIEW**: No unintended changes, no secrets
5. **WARNING REVIEW**: Fix critical warnings before commit
6. **VERIFICATION SCRIPT PATTERN**: Handle errors per-test, not exit on first failure

---

## Implementation

### 1. Build Verification

```bash
# Backend
cd backend/VendorMdm.Api
dotnet build --configuration Release
# Expected: Build succeeded, 0 Error(s)

# Frontend
cd frontend
npm run build
# Expected: ✓ built in ~4s, 0 errors
```

### 2. Migration Size Check

```bash
ls -lh backend/VendorMdm.Api/Migrations/*.cs | grep -v Designer | grep -v Snapshot
# All files must be < 50KB
# If any file > 50KB, STOP and split migration
```

### 3. Alignment Verification

```bash
./scripts/verify-alignment.sh
# Expected: ✓ ALL CHECKS PASSED
```

### 4. Git Status Review

```bash
git status
# Review all changed files
# Ensure no unintended changes
# Verify no sensitive data (keys, passwords)

# Check for secrets
git diff --staged | grep -i "password\|secret\|key\|token"
# Should return nothing
```

### 5. Warning Review

```bash
# Review all build warnings
# Fix critical warnings immediately
# Document acceptable warnings in commit message
```

### 6. Verification Script Pattern

```bash
# ❌ DON'T: Exit on first error (stops script early)
set -e
curl http://localhost:5001/health  # Fails → script stops

# ✅ DO: Accumulate failures, exit at end
FAIL_COUNT=0

if ! curl -s http://localhost:5001/health; then
    echo "FAIL: Health check"
    ((FAIL_COUNT++))
fi

if ! dotnet build --configuration Release; then
    echo "FAIL: Build failed"
    ((FAIL_COUNT++))
fi

# ... more tests ...

if [ $FAIL_COUNT -gt 0 ]; then
    echo "FAILED: $FAIL_COUNT tests"
    exit 1
else
    echo "PASSED: All tests"
    exit 0
fi
```

---

## Complete Pre-Commit Checklist

```bash
#!/bin/bash
# scripts/pre-commit-check.sh

FAIL_COUNT=0

echo "=== Pre-Commit Verification ==="

# 1. Build Backend
echo "→ Building backend..."
if ! dotnet build backend/VendorMdm.Api --configuration Release -v q; then
    echo "FAIL: Backend build"
    ((FAIL_COUNT++))
fi

# 2. Build Frontend
echo "→ Building frontend..."
if ! npm run build --prefix frontend; then
    echo "FAIL: Frontend build"
    ((FAIL_COUNT++))
fi

# 3. Migration Size
echo "→ Checking migration sizes..."
for file in backend/VendorMdm.Api/Migrations/*.cs; do
    if [[ ! "$file" =~ Designer && ! "$file" =~ Snapshot ]]; then
        size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file")
        if [ "$size" -gt 51200 ]; then
            echo "FAIL: Migration too large: $file ($size bytes)"
            ((FAIL_COUNT++))
        fi
    fi
done

# 4. No Secrets
echo "→ Checking for secrets..."
if git diff --staged | grep -iE "password|secret|apikey|token" | grep -v "# "; then
    echo "WARN: Possible secrets in staged changes"
fi

# 5. Summary
echo ""
if [ $FAIL_COUNT -gt 0 ]; then
    echo "❌ FAILED: $FAIL_COUNT checks failed"
    exit 1
else
    echo "✅ PASSED: All pre-commit checks"
    exit 0
fi
```

---

## Agent Behavior

**Before Commit**:
1. ✅ Run all pre-commit checks
2. ✅ Report any failures to user
3. ✅ MUST NOT commit if checks fail
4. ✅ Suggest fixes for any issues found

**Exceptions**:
- Hotfixes may skip alignment verification if time-critical
- Must be explicitly approved by user
- Must be documented in commit message

---

## Database Migration Deployment

| Environment | Database | How to Apply |
|-------------|----------|--------------|
| **Local** | SQLite | `dotnet ef database update` (direct) |
| **Azure** | SQL Server | **GitHub Actions ONLY** |

**NEVER**:
- ❌ Create manual SQL scripts for Azure
- ❌ Run EF migrations directly against Azure SQL
- ❌ Use Azure Portal SQL Query Editor for schema changes

**ALWAYS**:
- ✅ Commit migration files to git
- ✅ Merge to `develop` (or target branch)
- ✅ Trigger "Deploy Database Migrations" workflow in GitHub Actions

---

## Anti-Patterns

❌ Committing with build errors
❌ Migrations > 50KB (split them)
❌ Committing secrets or credentials
❌ Skipping verification scripts
❌ Using `set -e` in verification scripts (exits on first failure)

---

## Reference

- **Golden Rules**: Section 8
- **Build Hygiene**: [build-hygiene-standard.md](build-hygiene-standard.md)
- **CI/CD**: [cicd-setup-standards.md](cicd-setup-standards.md)
- **Git Branching**: [git-branching-sap-standards.md](git-branching-sap-standards.md)
