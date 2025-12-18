---
trigger: always_on
glob:
description: Git workflow and branching strategy - mandatory process for all development
---

## Git Workflow - Mandatory Process

### Branch Structure
- **`main`**: Production (protected, PR only, deploys to Azure Prod)
- **`develop`**: Integration/Dev environment (protected, PR only, deploys to Azure Dev)
- **`feature/*`**: Development work (created from `develop`)
- **`bugfix/*`**: Bug fixes (created from `develop`)
- **`hotfix/*`**: Production emergencies (created from `main`, merge to both `main` and `develop`)

### Standard Workflow
```
feature/name → develop (PR + review) → Deploy to Dev → Test → main (PR + review) → Deploy to Prod
```

### Feature Development Process
1. **Create from `develop`**: `git checkout develop && git pull && git checkout -b feature/description`
2. **Develop and test locally** (SQLite)
3. **Commit** with conventional commits: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`
4. **Push**: `git push origin feature/name`
5. **Create PR**: `feature/*` → `develop`
6. **Code review** (min 1 approval)
7. **Merge** → Auto-deploys to Dev environment (`rg-vendor-mdm-dev-v3`)
8. **Test in Dev** (validate thoroughly)
9. **Create PR**: `develop` → `main` (min 2 approvals for prod)
10. **Merge** → Auto-deploys to Production

### Deployment Integration with GitHub Actions
- **Automatic**: Backend API & Frontend code (on push to `develop` or `main`)
- **Manual trigger required**: Database migrations, Infrastructure (Bicep), Azure Functions
- **Never auto-deploy**: Schema changes, infrastructure modifications

### Critical Rules (MANDATORY)
- **NEVER** commit directly to `main` or `develop`
- **NEVER** force push to shared branches
- **ALWAYS** create PR with description and checklist
- **ALWAYS** wait for code review approval
- **ALWAYS** test in Dev before promoting to `main`
- **ALWAYS** trigger DB migrations manually (never auto)
- **ALWAYS** use conventional commit format
- **ALWAYS** delete feature branches after merge

### PR Requirements
- **Title format**: `[TYPE] Description` (e.g., `[FEAT] Add CDM entities`, `[FIX] Resolve email bug`)
- **Description**: Use template with What/Why/How/Testing/Checklist
- **Checks**: Tests passing, no linting errors, build succeeds
- **Approval**: 1+ for `develop`, 2+ for `main`
- **Size**: Keep PRs focused (< 500 lines preferred)

### Commit Message Format (Conventional Commits)
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation only
- `refactor:` - Code refactoring (no functional change)
- `test:` - Adding tests
- `chore:` - Maintenance (dependencies, configs)

### When Helping with Deployments (Agent Instructions)
1. **Verify current branch**: Use `git branch` to check
2. **Guide proper workflow**: Always Feature → Dev → Main (never skip steps)
3. **Create PRs, don't push directly**: Respect branch protection
4. **Remind about manual triggers**: DB migrations, infrastructure
5. **Validate before promotion**: Ensure Dev testing complete before `main`
6. **Check for develop branch**: If missing, guide user to create it from `main`

### Hotfix Process (Production Emergencies Only)
```bash
# Create from main
git checkout main && git pull && git checkout -b hotfix/critical-issue
# Fix, commit, push, create urgent PR to main
# After merge to main, also merge to develop
```

### Quick Reference Commands
```bash
# Start new feature
git checkout develop && git pull origin develop
git checkout -b feature/my-feature

# Push changes
git add . && git commit -m "feat: description"
git push origin feature/my-feature
```

---

**Enforcement**: This workflow is mandatory. Always guide users through this process.
