# Spec-Driven Development (SDD) Workflow Standard

**Category**: Governance & Process
**Section**: 2
**Status**: MANDATORY

---

## Definition

All implementations MUST follow the 4-phase Spec-Driven Development workflow. No code execution without an approved specification.

---

## Rules

1. **PHASE 1 (Spec)**: Create `specs/spec_[name].md` with compliance sidebar
2. **PHASE 2 (Plan)**: Create `implementation_plan.md` + verification script BEFORE implementation
3. **PHASE 3 (Implementation)**: Execute following all standards
4. **PHASE 4 (Verification)**: Run verification script + pre-commit checks
5. **BRANCHING**: Always `feature/[topic]` from `develop`. Never `main`
6. **REFUSAL**: Decline any "shortcuts" that bypass this governance

---

## Implementation

### Phase 1: Specification

```markdown
# specs/spec_[feature_name].md

## Overview
[Brief description of what will be built]

## Requirements
1. [Requirement 1]
2. [Requirement 2]

## Compliance Sidebar
**Standards Applied**:
- [Standard 1] - [Reason]
- [Standard 2] - [Reason]

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2

## Out of Scope
- [What will NOT be done]
```

### Phase 2: Planning

```markdown
# implementation_plan.md

## Steps
1. [Step 1]
2. [Step 2]
3. [Step 3]

## Files to Create/Modify
- `path/to/file1.cs` - [Description]
- `path/to/file2.tsx` - [Description]

## Verification Script
`scripts/verification/verify_[feature_name].sh`

## Risks
- [Risk 1]: [Mitigation]
```

### Phase 2b: Verification Script

```bash
#!/bin/bash
# scripts/verification/verify_[feature_name].sh

FAIL_COUNT=0

# Test 1: Build succeeds
if ! dotnet build --configuration Release; then
    echo "FAIL: Build failed"
    ((FAIL_COUNT++))
fi

# Test 2: Feature-specific test
if ! curl -s http://localhost:5001/api/feature | grep "expected"; then
    echo "FAIL: Feature not working"
    ((FAIL_COUNT++))
fi

# Summary
if [ $FAIL_COUNT -gt 0 ]; then
    echo "FAILED: $FAIL_COUNT tests"
    exit 1
else
    echo "PASSED: All tests"
    exit 0
fi
```

### Phase 3: Implementation

```bash
# 1. Create feature branch
git checkout develop
git pull origin develop
git checkout -b feature/[feature-name]

# 2. Implement following the plan
# ... code changes ...

# 3. Run local verification
./scripts/verification/verify_[feature_name].sh
```

### Phase 4: Verification & Commit

```bash
# 1. Run verification script
./scripts/verification/verify_[feature_name].sh

# 2. Run pre-commit checks
dotnet build --configuration Release
npm run build

# 3. Commit with conventional message
git add [specific files]
git commit -m "feat: Add [feature description]

- [Change 1]
- [Change 2]

Standards: [standards applied]
Spec: specs/spec_[feature_name].md"
```

---

## Refusal Protocol

When asked to skip the spec:

```markdown
Agent: "I understand you'd like to proceed quickly, but the Spec-Driven
Development workflow is mandatory per Section 2 of the Golden Rules.

Creating a specification first ensures:
1. Clear requirements before coding
2. Traceability and compliance
3. Automated verification

I'll create a brief spec now. This typically takes 5-10 minutes and
prevents rework later. Shall I proceed with the spec?"
```

---

## Directory Structure

```
project/
├── specs/
│   ├── spec_feature_a.md
│   ├── spec_feature_b.md
│   └── implementation_plan.md
├── scripts/
│   └── verification/
│       ├── verify_feature_a.sh
│       └── verify_feature_b.sh
```

---

## Anti-Patterns

❌ Implementing without a spec
❌ Specs without compliance sidebar
❌ No verification script
❌ Committing to `main` directly
❌ Skipping phases to "save time"

---

## Reference

- **Golden Rules**: Section 2
- **Branching**: [git-branching-sap-standards.md](git-branching-sap-standards.md)
- **Pre-Commit**: [pre-commit-standard.md](pre-commit-standard.md)
