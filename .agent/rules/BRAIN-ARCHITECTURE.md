# Brain Architecture: Documentation Hierarchy

**Purpose**: Defines the single-source-of-truth documentation structure for all agents (Claude & Antigravity)

---

## Hierarchy Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         ENTRY POINTS                                │
│                    (Pointers Only - No Content)                     │
├─────────────────────────────────────────────────────────────────────┤
│  CLAUDE.md ──────────────────────┐                                  │
│  (3 lines)                       │                                  │
│                                  ↓                                  │
│  MEMORY.md ──────────────────────→  .agent/rules/moderngoldenrules.md
│  (30 lines)                                                         │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ↓
┌─────────────────────────────────────────────────────────────────────┐
│              MASTER AUTHORITY (Executive Directive)                 │
│                  moderngoldenrules.md                               │
├─────────────────────────────────────────────────────────────────────┤
│  Section 0:  Zero Data Loss Policy                                  │
│  Section 1:  Compliance Logic                                       │
│  Section 2:  SDD Workflow                                           │
│  Section 3:  Performance DNA                                        │
│  Section 4:  Standards Brain (30 standards) ─────→ (references)     │
│  Section 5:  Build Hygiene                                          │
│  Section 6:  Architecture DNA                                       │
│  Section 7:  Security Standards                                     │
│  Section 8:  Pre-Commit Protocol                                    │
│  Section 9:  Warning Hygiene                                        │
│  Section 10: Retrospective Governance ───────────→ (references)     │
│  Section 11: EDA Governance                                         │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                    ┌──────────────┴──────────────┐
                    ↓                             ↓
┌───────────────────────────────┐  ┌─────────────────────────────────┐
│   DETAILED STANDARDS          │  │   ORGANIZATIONAL MEMORY         │
│   .agent/rules/standards/     │  │   .agent/retrospectives/        │
│   (30 files in 6 categories)  │  │                                 │
├───────────────────────────────┤  ├─────────────────────────────────┤
│                               │  │ INDEX.md (Top Learnings)        │
│ Category 1: Architecture (4)  │  │ active/*.md (Current Quarter)   │
│ Category 2: Core Dev (4)      │  │ archived/*.md (Past Quarters)   │
│ Category 3: Security (4)      │  │                                 │
│ Category 4: Integration (5)   │  │ ↑ Feeds learnings BACK to       │
│ Category 5: Operations (6)    │  │   Golden Rules (Section 10)     │
│ Category 6: Governance (7)    │  │                                 │
│                               │  │                                 │
│ See standards/README.md       │  └─────────────────────────────────┘
└───────────────────────────────┘
```

---

## Standards Organization (30 total in 6 categories)

### Category 1: Architecture & Design (4)
- hexagonal-architecture-standards.md
- data-model-standards.md
- ontology-modeling-standard.md
- repository-pattern-standard.md

### Category 2: Core Development (4)
- result-pattern-standard.md
- logging-standard.md
- state-machine-standard.md
- event-driven-architecture-standard.md

### Category 3: Security & Compliance (4)
- security-architecture.md
- audit-log-integration-standards.md
- soft-delete-standard.md
- gdpr-pii-standard.md

### Category 4: Integration & Infrastructure (5)
- sap-integration-standard.md
- file-storage-standard.md
- email-service-standard.md
- multi-tenancy-standard.md
- data-residency-standard.md

### Category 5: Operations & Quality (6)
- cicd-setup-standards.md
- database-migration-standards.md
- git-branching-sap-standards.md
- rate-limiting-standard.md
- performance-generated-columns.md
- ui-design-standards.md

### Category 6: Governance & Process (7)
- zero-data-loss-standard.md (Section 0)
- compliance-logic-standard.md (Section 1)
- sdd-workflow-standard.md (Section 2)
- build-hygiene-standard.md (Section 5)
- pre-commit-standard.md (Section 8)
- warning-hygiene-standard.md (Section 9)
- retrospective-standard.md (Section 10)

---

## Layer Definitions

| Layer | File(s) | Purpose | Content |
|-------|---------|---------|---------|
| **Pointer** | CLAUDE.md, MEMORY.md | Entry points | Reference to Golden Rules ONLY |
| **Master** | moderngoldenrules.md | Executive Directive | 12 sections of rules |
| **Standards** | standards/*.md (30 files) | Task-specific details | Detailed implementation rules |
| **Memory** | retrospectives/*.md | Organizational learning | Learnings feed back to Master |

---

## Key Principles

### 1. Single Source of Truth
- **ALL rules** live in `.agent/rules/`
- **NO duplication** between files
- **NO rules** in CLAUDE.md or MEMORY.md

### 2. Pointer Files (CLAUDE.md, MEMORY.md)
- Contain ONLY references to Golden Rules
- Maximum 30 lines
- NO actual rules or standards

### 3. Golden Rules (moderngoldenrules.md)
- Master Authority for all agent behavior
- References detailed standards via Section 4
- Owns all governance (SDD, Pre-Commit, Security)
- Section 10 = Checklist referencing standards

### 4. Standards (standards/*.md)
- Task-specific detailed rules
- Referenced BY Golden Rules Section 4
- **30 files** in **6 categories**
- Each section has exactly ONE standard

### 5. Retrospectives (retrospectives/*.md)
- Organizational memory
- Learnings feed BACK into Golden Rules
- Quarterly cleanup cycle

### 6. Architecture Maintenance (MANDATORY)
When adding new patterns/standards:
1. Update `BRAIN-ARCHITECTURE.md` (this file)
2. Update `standards/README.md` (standards index)
3. Update `moderngoldenrules.md` Section 4 (Standards Brain)
**Rule**: NO orphan patterns or standards allowed

---

## File Locations

```
/CLAUDE.md                              ← Pointer (3 lines)
/.claude/memory/MEMORY.md               ← Pointer (30 lines)
/.agent/rules/
    ├── BRAIN-ARCHITECTURE.md           ← This file
    ├── moderngoldenrules.md            ← Master Authority
    └── standards/
        ├── README.md                   ← Standards index
        │
        │── Architecture & Design (4)
        ├── hexagonal-architecture-standards.md
        ├── data-model-standards.md
        ├── ontology-modeling-standard.md
        ├── repository-pattern-standard.md
        │
        │── Core Development (4)
        ├── result-pattern-standard.md
        ├── logging-standard.md
        ├── state-machine-standard.md
        ├── event-driven-architecture-standard.md
        │
        │── Security & Compliance (4)
        ├── security-architecture.md
        ├── audit-log-integration-standards.md
        ├── soft-delete-standard.md
        ├── gdpr-pii-standard.md
        │
        │── Integration & Infrastructure (5)
        ├── sap-integration-standard.md
        ├── file-storage-standard.md
        ├── email-service-standard.md
        ├── multi-tenancy-standard.md
        ├── data-residency-standard.md
        │
        │── Operations & Quality (6)
        ├── cicd-setup-standards.md
        ├── database-migration-standards.md
        ├── git-branching-sap-standards.md
        ├── rate-limiting-standard.md
        ├── performance-generated-columns.md
        ├── ui-design-standards.md
        │
        │── Governance & Process (7)
        ├── zero-data-loss-standard.md
        ├── compliance-logic-standard.md
        ├── sdd-workflow-standard.md
        ├── build-hygiene-standard.md
        ├── pre-commit-standard.md
        ├── warning-hygiene-standard.md
        └── retrospective-standard.md

/.agent/retrospectives/
    ├── INDEX.md                        ← Top learnings
    ├── active/                         ← Current quarter
    └── archived/                       ← Past quarters
```

---

## Agent Behavior

### Before Every Task
1. Read `CLAUDE.md` → Follow pointer to Golden Rules
2. Read `moderngoldenrules.md` (Executive Directive)
3. Read relevant standard from Section 4 (based on task type)
4. Check `retrospectives/INDEX.md` for learnings

### After Significant Work
1. Document learnings in retrospective
2. Update INDEX.md with top findings
3. Apply learnings to Golden Rules immediately
4. Mark as applied in INDEX.md

---

## Anti-Patterns (FORBIDDEN)

❌ Duplicating rules in CLAUDE.md
❌ Duplicating rules in MEMORY.md
❌ Creating new rule files outside `.agent/rules/`
❌ Leaving "Pending" brain rule updates
❌ Skipping retrospective documentation
❌ Creating patterns without standards

---

**End of Brain Architecture**
