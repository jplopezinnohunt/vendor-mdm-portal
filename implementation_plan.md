# Implementation Plan: CI/CD Governance Alignment

## Goal
Align the CI/CD pipeline with the **Modern Golden Rules** (Rule 11, Rule 6, Rule 15).

## Proposed Changes

### 1. New PR Validation Gate
#### [NEW] [.github/workflows/verify-pr.yml](file:///Users/jplopez/projects/vendor-mdm-portal/.github/workflows/verify-pr.yml)
- Triggers on Pull Requests to `develop` and `main`.
- Jobs:
  - `backend-validation`: Runs `dotnet restore`, `dotnet build`, and `dotnet test`.
  - `frontend-validation`: Runs `npm install`, `npm run build`, and `npm run lint`.

### 2. Multi-Branch Backend Deployment
#### [MODIFY] [.github/workflows/deploy-backend-api.yml](file:///Users/jplopez/projects/vendor-mdm-portal/.github/workflows/deploy-backend-api.yml)
- Add `develop` to `push.branches`.
- Implement environment-based logic for `app-name`.
  - `develop` -> `app-vendor-mdm-api-dev`
  - `main` -> `app-vendor-mdm-api-prod` (Standardizing naming)

### 3. Multi-Branch Frontend Deployment
#### [MODIFY] [.github/workflows/azure-static-web-apps.yml](file:///Users/jplopez/projects/vendor-mdm-portal/.github/workflows/azure-static-web-apps.yml)
- Add `develop` to `push.branches` and `pull_request.branches`.
- This ensures dev environment is updated on merge to develop.

### 4. Database Migration Alignment
#### [MODIFY] [.github/workflows/deploy-database-migrations.yml](file:///Users/jplopez/projects/vendor-mdm-portal/.github/workflows/deploy-database-migrations.yml)
- Add `develop` to `push.branches`.
- Update logic to default to the appropriate environment based on branch.

## Verification Plan

### Automated Verification
#### [NEW] [scripts/verification/verify_cicd_alignment.sh](file:///Users/jplopez/projects/vendor-mdm-portal/scripts/verification/verify_cicd_alignment.sh)
- Script to:
  1. Check foresistenza of `verify-pr.yml`.
  2. Verify `develop` branch is present in all CI/CD triggers.
  3. Validate dynamic app-name logic in backend workflow.

## Compliance Sidebar
- **Rule 11 (Branching)**: Explicitly adding `develop` support.
- **Rule 6 (Execution)**: PR Gate enforces automated tests.
- **Rule 15 (Hygiene)**: Standardizing workflow dependencies (.NET 8.0.x).
