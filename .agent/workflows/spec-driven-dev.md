---
description: Standard workflow for Spec-Driven Development. USE THIS for all feature requests or complex bugfixes.
---

# Spec-Driven Development Workflow

Follow this strict process for every non-trivial task.

## Phase 1: Specification (The "What")

1. **Context Check & Golden Rules**:
   - **MANDATORY**: Read `.agent/rules/modern-golden-rules.md`.
   - Read `.agent/rules/standards/ontology-modeling-standard.md` (if dealing with Entities).
   - If UI task: Read `.agent/rules/standards/ui-design-standards.md`.

2. **Ontology Alignment (NEW)**:
   - Check `specs/ontology_registry.md` (The Master Ontology).
   - **Decision**: Does this feature modify an existing Concept or introduce a new one?
   - If NEW: You must propose the Concept definition first.

3. **Draft Specification**:
   - Create `specs/spec_[short_description].md`.
   - **Compliance Section**: You MUST list which Best Practices apply.
   - **Ontology Section**: Explicitly define the "Concepts" involved and their "Origin Context" for this feature.
   - **Crucial**: Text-based UI Mockup / JSON Schema.
   - **Crucial**: Acceptance Criteria.

4. **User Review**:
   - Pause and ask the user to review the Spec.
   - **Do not proceed** until the user says "Approved".

## Phase 2: Planning & Verification Prep (The "How")

1. **Draft Implementation Plan**:
   - Create/Update `implementation_plan.md`.
   - Map Spec to specific files.

2. **Create Verification Script (MANDATORY)**:
   - Create a script in `scripts/verification/` (e.g., `verify_task_001.sh` or `verify_task_001.ts`).
   - The script must be **executable** and **automated**.
   - *Example (API)*: `curl -f -X POST ...`
   - *Example (UI)*: `npx playwright test ...` or (if manual) a strict checklist echoing the script.
   - **Goal**: You must be able to run this script to prove "It works".

3. **User Review**:
   - Ask user to approve the Plan and the Verification Script.

## Phase 3: Execution

1. **Create Branch**:
   - Run `git checkout develop`.
   - Run `git pull origin develop`.
   - Run `git checkout -b feature/[task_name]`.
   - **Rule**: Never work directly on `main` or `develop`.

2. **Code**: Implement changes based *typically* on the Plan.
3. **Stop Condition**: If you find the Spec is wrong, **STOP**. Go back to Phase 1. Do not hack around it.

## Phase 4: Verification

1. **Run Script**: Execute the verification script from Phase 2.
2. **Evidence**: Capture output/screenshot.
3. **Walkthrough**: Create `walkthrough.md` with the evidence.

## Phase 5: Deployment

1. **Check Drift**: Compare local Bicep vs Azure Portal (if infra).
2. **Deploy**: Run the approved deployment command.

---

## Feature Completeness Checklist

Before marking a feature as "done", verify ALL items below:

### Backend Checklist

- [ ] **API Endpoints Implemented**
  - All endpoints defined in spec are implemented
  - Correct HTTP methods (GET, POST, PUT, DELETE)
  - Proper route naming and versioning

- [ ] **Error Handling Complete**
  - Try/catch blocks around all async operations
  - Proper HTTP status codes (400, 404, 500)
  - Meaningful error messages returned
  - Errors logged for debugging

- [ ] **Validation Implemented**
  - Input validation on all endpoints
  - DTO validation with data annotations
  - Business rule validation
  - Proper validation error messages

- [ ] **Tests Written** (if applicable)
  - Unit tests for business logic
  - Integration tests for API endpoints
  - Test coverage > 70%

- [ ] **Swagger Documentation Updated**
  - Endpoints documented with XML comments
  - Request/response examples provided
  - Authentication requirements documented

### Frontend Checklist

- [ ] **UI Components Implemented**
  - All screens/components from spec are built
  - Follows UI design standards
  - Responsive design (mobile, tablet, desktop)
  - Dark mode supported

- [ ] **Error Handling Complete**
  - Try/catch around ALL async operations
  - 10-second timeout on ALL API calls
  - Error messages displayed to user
  - Error messages are dismissible

- [ ] **Loading States Implemented**
  - Loading spinner during async operations
  - Buttons disabled during submission
  - Skeleton screens for initial load
  - Loading text is descriptive

- [ ] **Success/Error Messages Implemented**
  - Success message on completion (green banner)
  - Error message on failure (red banner)
  - Messages auto-dismiss (success) or manually dismissible (error)
  - Retry button when applicable

- [ ] **Empty States Implemented**
  - Empty state when no data
  - Relevant icon and message
  - CTA button when applicable
  - Helpful description

- [ ] **Responsive Design Verified**
  - Works on mobile (< 640px)
  - Works on tablet (640-1024px)
  - Works on desktop (> 1024px)
  - Touch targets 44x44px minimum

### Integration Checklist

- [ ] **Backend + Frontend Tested Together**
  - API calls work from frontend
  - Data flows correctly
  - Error handling works end-to-end
  - Success paths verified

- [ ] **CORS Configured**
  - Frontend can call backend
  - No CORS errors in console
  - Proper headers configured

- [ ] **Authentication Works**
  - Login flow functional
  - Protected routes work
  - Session management correct
  - Logout works

- [ ] **Authorization Works**
  - Role-based access control
  - Permissions enforced
  - Unauthorized access blocked

### Deployment Checklist

- [ ] **Builds Locally (0 errors)**
  - Backend builds successfully
  - Frontend builds successfully
  - No TypeScript errors
  - No C# errors

- [ ] **Migrations < 50KB**
  - All migrations under 50KB
  - Migrations tested locally
  - Migrations tested in Azure DEV

- [ ] **Deployed to DEV**
  - Code pushed to develop branch
  - GitHub Actions successful
  - Deployment verified

- [ ] **Verified in DEV**
  - Feature works in DEV environment
  - No console errors
  - No API errors
  - Critical paths tested

- [ ] **Ready for PROD**
  - All checklists complete
  - User acceptance obtained
  - Release notes prepared

### Agent Behavior

**Before Marking Feature as Done**:
1. ✅ Review this checklist
2. ✅ Verify ALL applicable items are complete
3. ✅ Report completion status to user
4. ✅ Highlight any incomplete items
5. ✅ Create issues for deferred items

**Reporting Format**:
```
✅ Feature Completeness Report

Backend: 6/6 ✅
Frontend: 5/5 ✅
Integration: 4/4 ✅
Deployment: 5/5 ✅

TOTAL: 20/20 ✅ READY FOR PRODUCTION

Deferred Items: None
```

**If Incomplete**:
```
⚠️ Feature Completeness Report

Backend: 5/6 ⚠️
  ❌ Swagger documentation missing

Frontend: 5/5 ✅
Integration: 4/4 ✅
Deployment: 4/5 ⚠️
  ❌ Not yet deployed to DEV

TOTAL: 18/20 ⚠️ NOT READY

Action Required:
1. Add Swagger documentation
2. Deploy to DEV and verify
```

**Agent MUST NOT**:
- ❌ Mark feature as done if checklist incomplete
- ❌ Skip items without user approval
- ❌ Deploy to PROD with incomplete checklist
