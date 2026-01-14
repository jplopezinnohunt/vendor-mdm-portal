# Specification: CI/CD Governance Alignment

## Problem Statement
Current CI/CD workflows are disconnected from the mandatory project governance:
1.  They only trigger on `main`, ignoring the `develop` branch required by Rule 11.
2.  There is no automated quality gate (lint, test, build) for Pull Requests.
3.  Deployments hardcode the `dev` environment even when triggered from `main`.

## Proposed Solution
Align the DevOps pipeline with the **Modern Golden Rules**:
-   **Multi-Branch Deployment**: `develop` deploys to Dev/Dev resources; `main` deploys to Prod resources.
-   **Mandatory PR Gate**: A new `verify-pr.yml` workflow to run all tests and builds.
-   **Rule Compliance**: Ensure all workflows use consistent .NET/Node versions.

## Compliance
- **Rule 11 (Branching)**: Workflows will now support `develop` and `feature/*`.
- **Rule 6 (Coding Standards)**: PR gate will enforce `dotnet test` and `npm run build`.
- **Rule 4 (Azure Truth)**: Infrastructure deployment remains via Azure CLI/Actions.
- **Rule 15 (Hygiene)**: Removal of any inline/CDN artifacts in the build process.

## Acceptance Criteria
- [ ] Pull Request to `develop` or `main` triggers a verification workflow.
- [ ] Verification workflow fails if `dotnet test` or `npm run build` fails.
- [ ] Merge to `develop` triggers deployment to `app-vendor-mdm-api-dev`.
- [ ] Merge to `main` triggers deployment to the production instance (to be configured).
- [ ] Workflows centralized to use reusable secrets.

## Verification Plan (Phase 2 Preview)
- Validation via a dummy PR to triggers the new verification workflow.
- Mock deployment run using `workflow_dispatch`.
