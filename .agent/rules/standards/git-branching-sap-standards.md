# Vendor MDM Platform - Branching Strategy

**Version:** v2.17 (Official Standard)  
**Date:** December 17, 2025  
**Interactive View:** Visit `[Portal URL]/admin/strategy` to see the live visualization.

## 1. Introduction
This document defines the comprehensive branching strategy for the Vendor MDM Platform, visualizing the full deployment flow: from code in Git, through Azure environments (App Service/Functions), to the data connection with the SAP landscape.

### Core Principles
1. **Azure PROD + SAP P01 always synchronized**: The production code must always be compatible with the live SAP environment.
2. **Azure DEV consumes data from SAP D01**: Development and integration testing happens against the SAP Development environment.
3. **Staging (QA) validates against SAP Q01**: User Acceptance Testing (UAT) is performed in Staging connected to SAP Quality Assurance.
4. **Full Cycle**: Code -> Deploy Azure -> Connect SAP.
5. **Hotfixes with fast-track**: A dedicated path exists for critical production fixes.

---

## 2. Process Roles & Actors
Understanding who is responsible for what is crucial for this lifecycle.

| Role | Responsibility |
| :--- | :--- |
| **Developer** | Responsible for technical implementation. Creates `feature/*` branches, writes code, runs local unit tests, and resolves merge conflicts. |
| **Tech Lead / Peer** | Quality guardian. Performs Code Reviews, approves Pull Requests (PRs) to `develop`, and ensures architecture/security standards. |
| **Release Manager** | Schedule manager. Decides when a version is cut (`release/*`), freezes code for QA, and coordinates deployments. |
| **Key User** | Business validator. Executes UAT in Staging (SAP Q01) and gives the 'Go/No-Go' for production. |
| **DevOps Engineer** | Infrastructure architect. Manages CI/CD pipelines, monitors Azure/SAP health, and executes final production releases. |

---

## 3. Branch Definitions

### `main` (Production)
*   **Type**: Production
*   **Environment**: Azure PRODUCTION (Connects to **SAP P01**)
*   **Description**: Production code. Must be 100% compatible with current SAP P01 configuration.
*   **Protection**: No direct commits, PR required (1 approval), CI/CD 100% passing, Only admins can force-push.

### `develop` (Integration)
*   **Type**: Integration
*   **Environment**: Azure DEV (Connects to **SAP D01**)
*   **Description**: Continuous integration. Place to test integrations with BAPIs/IDOCs in development.
*   **Protection**: No direct commits, PR required from `feature/*`, Auto-deploy to DEV.

### `feature/*` (Development)
*   **Type**: Development
*   **Environment**: Local / DEV (Connects to **SAP D01** or Mocks)
*   **Description**: Feature development. Can use SAP Mocks if D01 is unstable.
*   **Lifecycle**: Born from `develop` -> PR -> Merge to `develop` -> Delete.

### `release/*` (Staging/QA)
*   **Type**: Staging
*   **Environment**: Azure STAGING (Connects to **SAP Q01**)
*   **Description**: User Acceptance Testing (UAT). Validates custom logic works with real Q01 data.
*   **Lifecycle**: Born from `develop` -> QA in Staging -> Merge to `main` & `develop`.

### `hotfix/*` (Emergency)
*   **Type**: Emergency
*   **Environment**: Azure PRODUCTION (Connects to **SAP P01**)
*   **Description**: Urgent fix. Requires rapid validation of non-regression in SAP P01.
*   **Lifecycle**: Born from `main` -> Fix -> Merge to `main` & `develop`.

---

## 4. Operational Workflows

### Feature Development
1.  **Create Branch**: `git checkout -b feature/VEN-123-sap-sync develop` (From develop).
2.  **Develop**: `git commit -m 'feat(sap): consume BAPI_VENDOR_GET'` (Implement logic).
3.  **Pull Request**: Compare `feature` -> `develop` via GitHub UI.
4.  **Merge**: Squash & Merge (Auto deploy to Azure DEV).

### Release Process
1.  **Sync**: Confirm SAP transports are in Q01.
2.  **Freeze**: `git checkout -b release/v1.2.0 develop`.
3.  **Deploy Staging**: Manual Deploy -> Azure Staging (App points to SAP Q01).
4.  **UAT**: Users validate integration in Q01.
5.  **Release PROD**: Merge `release` -> `main`. Deploy to PROD same time SAP moves to P01.

### Emergency Hotfix
1.  **Start**: `git checkout -b hotfix/VEN-404-sap-error main`.
2.  **Fix**: `git commit -m 'fix: adjust payload for SAP'`.
3.  **Deploy**: Merge `hotfix` -> `main` (Urgent deploy to Azure PROD).
4.  **Sync**: Merge `hotfix` -> `develop` (Replicate fix to development).

---

## 5. Azure CI/CD & SAP Integration

| Branch | Azure Environment | SAP Connection | Trigger |
| :--- | :--- | :--- | :--- |
| `develop` | DEV | **SAP D01** | Automatic |
| `release/*` | STAGING | **SAP Q01** | Manual (QA) |
| `main` | PRODUCTION | **SAP P01** | Manual (Lead) |
| `hotfix/*` | PRODUCTION | **SAP P01** | Manual (Urgent) |

---

## 6. Conventional Commits
We follow the conventional commits specification:
*   `feat`: New feature (Minor)
*   `fix`: Bug fix (Patch)
*   `docs`: Documentation
*   `refactor`: Code change without logic change
*   `test`: Add or correct tests
*   `chore`: Maintenance, dependencies

### Commit Message Best Practice
**Do not just write a title.** Future pushes must include a detailed description:
```text
feat: add new vendor onboarding form

- Added form validation with Zod
- Integrated API endpoint /api/vendors
- Updated UI buttons to match brand colors
- Rationale: Required for VEN-123 user story
```

---

## 7. Hotfix Protocol (DETAILED)

**WHEN TO USE**: Critical production issues ONLY.

**Definition of Critical**:
- Production is down or severely degraded
- Data loss or corruption risk
- Security vulnerability
- Critical business process blocked

### Hotfix Process (Step-by-Step)

**1. Create Hotfix Branch from Main**:
```bash
# Ensure main is up to date
git checkout main
git pull origin main

# Create hotfix branch
git checkout -b hotfix/description main
# Example: git checkout -b hotfix/fix-migration-failure main
```

**2. Fix Issue (Minimal Changes Only)**:
```bash
# Make ONLY the necessary changes
# Do NOT add new features
# Do NOT refactor unrelated code

# Commit with clear message
git commit -m "fix: description of fix

HOTFIX: Critical issue description
IMPACT: What was broken
FIX: What was changed
TESTED: How it was verified"
```

**3. Test Locally**:
```bash
# Run alignment verification
./scripts/verify-alignment.sh
# Expected: ✓ ALL CHECKS PASSED

# Test the specific fix
# Verify no regressions
```

**4. Merge to Main AND Develop**:
```bash
# Merge to main (production)
git checkout main
git merge hotfix/description --no-ff -m "Merge hotfix: description"

# Merge to develop (keep in sync)
git checkout develop
git merge hotfix/description --no-ff -m "Merge hotfix: description"

# Push both branches
git push origin main
git push origin develop
```

**5. Tag Release**:
```bash
# Create patch version tag
git tag -a v1.0.x -m "Hotfix: description

CRITICAL FIX:
- Issue: ...
- Fix: ...
- Impact: ..."

git push origin v1.0.x
```

**6. Deploy and Verify**:
```bash
# Wait for Azure deployment (5-10 min)
# Monitor GitHub Actions

# Run deployment verification
./scripts/verify-deployment.sh
# Expected: ✓ DEPLOYMENT VERIFIED

# Verify the fix works in production
# Monitor for any issues
```

**7. Document and Communicate**:
```bash
# Create issue documenting the hotfix
# Update release notes
# Notify stakeholders
```

### Hotfix Checklist

**Before Starting**:
- [ ] Issue is truly critical (production down/data loss/security)
- [ ] Root cause identified
- [ ] Fix approach validated
- [ ] Stakeholders notified

**During Hotfix**:
- [ ] Branch created from main
- [ ] Minimal changes only (no features, no refactoring)
- [ ] Tested locally
- [ ] Alignment verification passed
- [ ] Commit message explains issue and fix

**After Hotfix**:
- [ ] Merged to main
- [ ] Merged to develop
- [ ] Tagged with version
- [ ] Deployed to production
- [ ] Deployment verified
- [ ] Issue documented
- [ ] Stakeholders notified

### Agent Behavior

When executing a hotfix, the agent MUST:
1. ✅ Verify issue is critical before proceeding
2. ✅ Create branch from main (not develop)
3. ✅ Make minimal changes only
4. ✅ Test locally before commit
5. ✅ Merge to BOTH main AND develop
6. ✅ Create tag with patch version
7. ✅ Verify deployment
8. ✅ Document the hotfix

**FORBIDDEN**:
- ❌ Adding new features during hotfix
- ❌ Refactoring unrelated code
- ❌ Merging only to main (must merge to develop too)
- ❌ Skipping testing
- ❌ Skipping deployment verification

### Hotfix vs Regular Fix

| Criteria | Hotfix | Regular Fix |
|----------|--------|-------------|
| **Severity** | Critical (prod down) | Non-critical |
| **Branch From** | main | develop |
| **Merge To** | main + develop | develop only |
| **Testing** | Minimal (focused) | Comprehensive |
| **Deployment** | Immediate | Next release |
| **Tag** | Patch version (v1.0.x) | Minor/Major version |

### Example Hotfix

**Scenario**: Database migration fails in production

```bash
# 1. Create hotfix branch
git checkout -b hotfix/fix-migration-failure main

# 2. Fix the migration
# Remove large migration, keep only critical ones

# 3. Commit
git commit -m "fix: Remove large migration causing Azure SQL timeout

HOTFIX: Database migration deployment failing
IMPACT: Features blocked from production
FIX: Removed 155KB migration, kept only critical migrations
TESTED: Local migration successful, Azure DEV deployment verified"

# 4. Test
./scripts/verify-alignment.sh

# 5. Merge to main
git checkout main
git merge hotfix/fix-migration-failure --no-ff

# 6. Merge to develop
git checkout develop
git merge hotfix/fix-migration-failure --no-ff

# 7. Tag
git tag -a v1.0.1 -m "Hotfix: Fix migration failure"

# 8. Push
git push origin main develop v1.0.1

# 9. Verify deployment
./scripts/verify-deployment.sh
```

---

## 8. FAQ

**Q: What if SAP D01 is down?**
A: Recommend using local Mocks in 'feature' branch to not stop development, but real integration is validated in 'develop'.

**Q: How to coordinate a release with SAP?**
A: 'release branch' must not go to PROD until SAP team confirms transports are ready for P01.

**Q: Can I point develop to SAP Q01?**
A: Not recommended. 'develop' is unstable and could pollute Q01 data used for formal testing.
