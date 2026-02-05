# Git & CI/CD Standards

---

## 4.1 Branch Strategy (SAP Aligned)

| Branch | Environment | SAP Connection | Trigger |
|--------|-------------|----------------|---------|
| `main` | PRODUCTION | SAP P01 | Manual |
| `develop` | DEV | SAP D01 | Automatic |
| `release/*` | STAGING | SAP Q01 | Manual |
| `feature/*` | Local/DEV | SAP D01/Mocks | PR to develop |
| `hotfix/*` | PRODUCTION | SAP P01 | Manual (urgent) |

---

## 4.2 Branching Rules

### Feature Development
```bash
# 1. Create branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/VEN-123-description

# 2. Develop and commit
git commit -m "feat: add new feature"

# 3. Push and create PR
git push origin feature/VEN-123-description
# Create PR: feature/VEN-123 → develop

# 4. After merge, auto-deploy to DEV
```

### Release Process
```bash
# 1. Confirm SAP transports are in Q01
# 2. Create release branch
git checkout -b release/v1.2.0 develop

# 3. Deploy to STAGING (manual)
# 4. UAT testing against Q01
# 5. Merge to main when SAP moves to P01
git checkout main
git merge release/v1.2.0 --no-ff
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin main --tags
```

### Hotfix Protocol
```bash
# 1. Create from main (not develop)
git checkout main
git pull origin main
git checkout -b hotfix/fix-critical-bug main

# 2. Fix (minimal changes only)
git commit -m "fix: critical bug description

HOTFIX: What was broken
IMPACT: Who was affected
FIX: What was changed"

# 3. Merge to BOTH main AND develop
git checkout main
git merge hotfix/fix-critical-bug --no-ff
git checkout develop
git merge hotfix/fix-critical-bug --no-ff

# 4. Tag and push
git tag -a v1.2.1 -m "Hotfix: description"
git push origin main develop v1.2.1
```

---

## 4.3 Conventional Commits

| Type | Description | Version |
|------|-------------|---------|
| `feat` | New feature | Minor |
| `fix` | Bug fix | Patch |
| `docs` | Documentation | - |
| `refactor` | Code restructure | - |
| `test` | Add/correct tests | - |
| `chore` | Maintenance | - |

### Commit Message Format
```text
feat: add vendor onboarding form

- Added form validation with Zod
- Integrated API endpoint /api/vendors
- Updated UI to match brand colors
- Rationale: Required for VEN-123 user story

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>
```

---

## 4.4 CI/CD Workflows

### Automatic Deployments
| Workflow | File | Trigger |
|----------|------|---------|
| Backend API | `deploy-backend-api.yml` | Push to main (backend changes) |
| Frontend | `azure-static-web-apps.yml` | Push to main (frontend changes) |

### Manual Deployments
| Workflow | File | When to Use |
|----------|------|-------------|
| Database | `deploy-database-migrations.yml` | Schema changes |
| Infrastructure | `deploy-infrastructure.yml` | Bicep changes |
| Functions | `azure-functions.yml` | Function apps |

---

## 4.5 Post-Deployment Verification

### Verification Steps
1. **Wait for Deployment** (5-10 minutes)
2. **Run Verification Script**
   ```bash
   ./scripts/verify-deployment.sh
   # Expected: ✓ DEPLOYMENT VERIFIED
   ```
3. **Manual Checks**
   - [ ] Backend Swagger accessible
   - [ ] Frontend loads
   - [ ] Login works
   - [ ] Critical features functional

### Smoke Tests
```bash
# Backend health
curl https://app-vendor-mdm-api-dev.azurewebsites.net/health
# Expected: {"status":"Healthy"}

# Frontend
curl https://thankful-field-0258f8110.3.azurestaticapps.net/
# Expected: 200 OK

# Swagger
curl https://app-vendor-mdm-api-dev.azurewebsites.net/swagger
# Expected: 200 OK
```

---

## 4.6 Rollback Procedures

### Code Rollback
```bash
# Revert the commit
git revert <commit-sha>
git push origin main
# GitHub Actions will auto-deploy reverted version
```

### Database Rollback
```bash
# Via Azure Portal SQL Query Editor
# Or trigger workflow with previous migration
```

---

## 4.7 GitHub Secrets Required

| Secret | Purpose |
|--------|---------|
| `AZURE_CREDENTIALS` | Service principal for Azure |
| `AZURE_APP_SERVICE_PUBLISH_PROFILE` | Backend deployment |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Frontend deployment |

---

## 4.8 Deployment Decision Tree

```
┌─ Code Change?
│
├─ Backend Code → Push to main → Auto-deploys
│
├─ Frontend Code → Push to main → Auto-deploys
│
├─ Database Schema → Manual workflow → "Deploy Database Migrations"
│
├─ Infrastructure (Bicep) → Manual workflow → "Deploy Azure Infrastructure"
│
└─ Azure Functions → Manual workflow → "Deploy Backend Artifacts"
```
