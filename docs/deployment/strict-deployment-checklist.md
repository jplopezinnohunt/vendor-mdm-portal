# Strict Deployment Verification Checklist

**Version**: 1.0  
**Effective Date**: 2026-01-10  
**Purpose**: To prevent compilation errors and regressions from reaching the `main` branch during merges.

---

## 🛑 CRITICAL RULE: "Build Before Push"

**NEVER** push a merge commit to `main` or `develop` without running a local build of **BOTH** frontend and backend immediately after the merge operation.

---

## 1. Pre-Merge Preparation (On Source Branch)
Before merging `develop` to `main`:

- [ ] **Clean Working Tree**: Ensure `git status` is clean.
- [ ] **Pull Latest**: Ensure source branch is up to date (`git pull origin develop`).
- [ ] **Local Build Verification**:
  ```bash
  # Backend
  dotnet build backend/VendorMdm.Api/VendorMdm.Api.csproj
  
  # Frontend
  cd frontend && npm run build && cd ..
  ```

## 2. Merge Execution (Local)
Perform the merge locally but **DO NOT PUSH** yet.

```bash
git checkout main
git pull origin main
git merge develop --no-ff -m "Merge develop into main: [Description]"
```

## 3. 🚨 Post-Merge Validation (MANDATORY)
**This is the step that catches merge conflicts and syntax errors.**

- [ ] **Backend Compilation Check**:
  > *Why? Merges can result in syntax errors, orphaned code, or duplicate logic that git auto-merge doesn't catch.*
  ```bash
  dotnet build backend/VendorMdm.Api/VendorMdm.Api.csproj
  # MUST return "0 Error(s)"
  ```

- [ ] **Frontend Build Check**:
  ```bash
  cd frontend && npm run build && cd ..
  # MUST complete without error
  ```

- [ ] **Verify Critical Files**:
  - Check files heavily modified in the merge (e.g., `ClaimsTransformationService.cs`, `AuthContext.tsx`).
  - Look for git conflict markers `<<<<<<<` if not using a merge tool.

## 4. Push to Remote
Only after Step 3 passes 100%:

```bash
git push origin main
```

## 5. Deployment Monitoring
**Do not assume success.**

1. Navigate to: [GitHub Actions](https://github.com/jplopezinnohunt/vendor-mdm-portal/actions)
2. **Verify ALL triggered workflows**:
   - `Azure Static Web Apps CI/CD` (Frontend)
   - `Deploy Backend API to Azure` (Backend)
   - `Deploy Database Migrations` (if applicable)
3. **Confirm Success**: Green checkmark ✅ on all.
4. **If Failed**:
   - Inspect build logs immediately.
   - Revert or fix in `hotfix` branch.

---

## Lesson Learned (2026-01-10)
**Incident**: Merge commit `7b9c8b6` introduced a syntax error in `ClaimsTransformationService.cs`.
**Root Cause**: The merge caused orphaned code lines (duplicate logic) that were not detected because `dotnet build` was not run **after** the local merge and **before** the push.
**Resolution**: Implemented this strict checklist to enforce post-merge validation.
