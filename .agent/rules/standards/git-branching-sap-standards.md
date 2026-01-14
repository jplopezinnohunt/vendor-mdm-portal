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

## 7. FAQ

**Q: What if SAP D01 is down?**
A: Recommend using local Mocks in 'feature' branch to not stop development, but real integration is validated in 'develop'.

**Q: How to coordinate a release with SAP?**
A: 'release branch' must not go to PROD until SAP team confirms transports are ready for P01.

**Q: Can I point develop to SAP Q01?**
A: Not recommended. 'develop' is unstable and could pollute Q01 data used for formal testing.
