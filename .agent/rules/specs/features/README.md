# Features (In-Progress Specs)

**Purpose**: Temporal specifications for active feature development.

---

## Lifecycle

```
1. Create Branch     →  feature/topic-name
2. Create Spec       →  features/topic-name.md
3. Implement         →  Per spec
4. PR & Merge        →  To develop/main
5. Update Solution   →  Move relevant content to solution/
6. Archive Spec      →  Delete or move to archive
```

---

## Naming Convention

```
features/
├── {topic-name}.md          ← Active feature spec
└── archive/                 ← Completed features (optional)
    └── {date}-{topic}.md
```

---

## Template

When starting a new feature, create `features/{topic-name}.md`:

```markdown
# Feature: {Topic Name}

**Branch**: feature/{topic-name}
**Status**: In Progress
**Started**: YYYY-MM-DD

## Objective

What this feature accomplishes.

## Requirements

- Requirement 1
- Requirement 2

## Scope

### In Scope
- Item 1

### Out of Scope
- Item 2

## Implementation Notes

Technical approach.

## Acceptance Criteria

- [ ] Criterion 1
- [ ] Criterion 2

## Solution Spec Updates

After completion, update:
- [ ] solution/CORE.md (if architecture changes)
- [ ] solution/FLOWS.md (if new workflow)
- [ ] solution/INTEGRATIONS.md (if new integration)
- [ ] functional/{ROLE}.md (if role flow changes)
```

---

## Current Active Features

*None at this time.*

---

## Completed (Ready to Archive)

| Feature | Branch | Merged | Solution Updated |
|---------|--------|--------|------------------|
| - | - | - | - |
