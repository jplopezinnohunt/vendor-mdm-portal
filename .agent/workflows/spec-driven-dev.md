---
description: Standard workflow for Spec-Driven Development. USE THIS for all feature requests or complex bugfixes.
---

# Spec-Driven Development Workflow

Follow this strict process for every non-trivial task.

## Phase 1: Specification (The "What")

1. **Context Check & Golden Rules**:
   - **MANDATORY**: Read `best_practices.md`.
   - Read `task.md`.
   - Read `user_rules` (Memo).
   - If UI task: Read `docs/ui_design_standards.md`.

2. **Draft Specification**:
   - Create `specs/spec_[short_description].md`.
   - **Compliance Section**: You MUST list which Best Practices apply (e.g., "Uses Hybrid Data Model", "Follows UI Pillar A & B").
   - **Crucial**: Text-based UI Mockup / JSON Schema.
   - **Crucial**: Acceptance Criteria.

3. **User Review**:
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
