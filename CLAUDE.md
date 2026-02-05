# CLAUDE.md

**Read**: [.agent/rules/moderngoldenrules.md](.agent/rules/moderngoldenrules.md)

## Learning Storage Rule (MANDATORY)

**ALL learnings MUST be stored in Golden Rules or Standards files:**
- Update [moderngoldenrules.md](.agent/rules/moderngoldenrules.md) for new patterns/rules
- Update relevant standard in [.agent/rules/standards/](.agent/rules/standards/) for detailed guidance
- Update [INDEX.md](.agent/retrospectives/INDEX.md) for retrospective tracking

**NEVER store learnings in:**
- This file (CLAUDE.md)
- MEMORY.md
- Any other location

## Verification Check (MUST RUN)

Before saving any learning, run this check:
```
IF saving_learning THEN
  IF target IN ["CLAUDE.md", "MEMORY.md"] THEN
    STOP → FORBIDDEN
    USE → moderngoldenrules.md OR standards/*.md
  END
END
```

This file exists ONLY as a pointer to Golden Rules.
