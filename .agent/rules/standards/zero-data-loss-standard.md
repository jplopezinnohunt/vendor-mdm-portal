# Zero Data Loss Standard

**Category**: Governance & Process
**Section**: 0
**Status**: MANDATORY

---

## Definition

Database files and user data MUST be treated as production-critical. Destructive operations require explicit written consent.

---

## Rules

1. **FORBIDDEN**: Deleting, resetting, or overwriting database files (`*.db`, `*.sqlite`) without explicit, written consent
2. **FORBIDDEN**: Recursive data directory deletions (`rm -rf`) without explicit approval
3. **ALWAYS** fix migration scripts instead of deleting databases
4. **ALWAYS** assume local data is production-critical test data

---

## Implementation

### Recovery Priority

```bash
# ❌ FORBIDDEN (unless explicitly approved)
rm -rf data/
rm *.db
rm *.sqlite

# ✅ CORRECT: Fix the migration
dotnet ef migrations remove
# Fix the issue in the migration code
dotnet ef migrations add FixedMigration
```

### Consent Protocol

Before any destructive database operation:

1. **Explicit Request**: User must explicitly request "Reset DB" or "Delete database"
2. **Confirmation**: Agent must confirm understanding before proceeding
3. **Backup Suggestion**: Agent should suggest backup first

```markdown
# Example Consent Flow

User: "The migration is failing, just delete the database"
Agent: "I understand you want to delete the database. Before I do this:
        1. This will DELETE ALL DATA in the database
        2. Would you like me to create a backup first?
        Please confirm with 'yes, delete the database' to proceed."
User: "yes, delete the database"
Agent: [Proceeds with deletion]
```

### Safe Migration Practices

```bash
# 1. First, try to fix the migration
dotnet ef migrations remove  # Remove problematic migration
# Edit the migration code to fix issues
dotnet ef migrations add CorrectedMigration

# 2. If schema is incompatible, create new migration
dotnet ef migrations add SchemaFix

# 3. Only as LAST RESORT with explicit consent
rm app.db  # REQUIRES USER CONSENT
dotnet ef database update
```

---

## Anti-Patterns

❌ Deleting database to "start fresh" without consent
❌ Using `rm -rf` on data directories
❌ Assuming test data is disposable
❌ Skipping backup before destructive operations

---

## Reference

- **Golden Rules**: Section 0
- **Related**: [database-migration-standards.md](database-migration-standards.md)
