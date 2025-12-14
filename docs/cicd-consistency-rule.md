# CI/CD Pipeline Consistency - MANDATORY RULE

## Core Principle
**All code pushed to `main` must result in passing CI/CD pipelines.**

## The Problem
GitHub workflows are failing because they require Azure infrastructure that isn't deployed yet:
- ❌ Deploy Backend Artifacts
- ❌ Azure Static Web Apps CI/CD

## Immediate Fix: Disable Workflows Until Ready

Workflows are disabled until Azure infrastructure is properly configured.

## Rule for Future Development

### Before Pushing to Main:
1. ✅ Local build succeeds (`dotnet build` = 0 errors)
2. ✅ GitHub Actions configured or disabled
3. ✅ After push, check https://github.com/jplopezinnohunt/vendor-mdm-portal/actions
4. ✅ All workflows must be green (or intentionally disabled)

### If Workflow Fails After Push:
1. Investigate immediately
2. Fix or disable the workflow
3. Push fix within 1 hour
4. Code is NOT production-ready until CI/CD passes

## Long-term: Enable Branch Protection
- Require pull requests
- Require status checks to pass
- No direct pushes to `main`
