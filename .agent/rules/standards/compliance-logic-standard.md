# Compliance Logic Standard

**Category**: Governance & Process
**Section**: 1
**Status**: MANDATORY

---

## Definition

All agent behavior MUST comply with the Golden Rules. External standards MUST be proactively loaded based on task type.

---

## Rules

1. **PRIMARY SOURCE**: `moderngoldenrules.md` is the "System Logic" - the single source of truth
2. **PROACTIVE LOADING**: When a task involves specific domains (UI, Data, Architecture), agents MUST read the relevant standard
3. **CITATION REQUIRED**: Every specification must cite which standard was followed
4. **ARCHITECTURE MAINTENANCE**: When adding new patterns/standards, all index files must be updated

---

## Implementation

### Brain Architecture

```
CLAUDE.md / MEMORY.md
       ↓ (pointer)
moderngoldenrules.md (Master Authority)
       ↓ (references)
standards/*.md (Detailed Standards)
       ↓ (feeds back)
retrospectives/*.md (Learnings)
```

### Proactive Standard Loading

```markdown
# Task: "Create a new React form"
# Agent MUST read: ui-design-standards.md

# Task: "Add a new database table"
# Agent MUST read: data-model-standards.md

# Task: "Implement authentication"
# Agent MUST read: security-architecture.md
```

### Citation in Specifications

```markdown
# specs/spec_vendor_form.md

## Compliance Sidebar

**Standards Applied**:
- UI Design Standards (Section 4.1) - Form patterns
- Security Architecture (Section 7) - Input validation
- Data Model Standards (Section 4.2) - Entity design

**Verification**: scripts/verification/verify_vendor_form.sh
```

### Architecture Maintenance Protocol

When adding new patterns or standards:

```bash
# 1. Create the standard file
touch .agent/rules/standards/new-pattern-standard.md

# 2. Update BRAIN-ARCHITECTURE.md (hierarchy diagram)
# 3. Update standards/README.md (standards index)
# 4. Update moderngoldenrules.md Section 4 (Standards Brain)

# Validation: All three files must reference the new standard
```

---

## Standard Categories

| Category | When to Load |
|----------|--------------|
| Architecture & Design | New features, refactoring |
| Core Development | Business logic, patterns |
| Security & Compliance | Auth, data protection |
| Integration & Infrastructure | External systems |
| Operations & Quality | CI/CD, deployments |
| Governance & Process | Workflow, process |

---

## Anti-Patterns

❌ Implementing without reading relevant standards
❌ Specifications without compliance sidebar
❌ Orphan standards not in index files
❌ Skipping architecture maintenance

---

## Reference

- **Golden Rules**: Section 1
- **Brain Architecture**: [BRAIN-ARCHITECTURE.md](../BRAIN-ARCHITECTURE.md)
- **Standards Index**: [README.md](README.md)
