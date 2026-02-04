# Warning Hygiene Standard

**Category**: Governance & Process
**Section**: 9
**Status**: MANDATORY

---

## Definition

Production builds MUST target zero warnings. All warnings must be categorized and addressed appropriately.

---

## Rules

1. **TARGET**: Zero warnings in production builds
2. **CATEGORIZE**: All warnings must be classified (Critical/Important/Minor)
3. **FIX CRITICAL**: Critical warnings must be fixed immediately
4. **FIX IMPORTANT**: Important warnings must be fixed before merge
5. **DOCUMENT MINOR**: Minor warnings must be documented in commit message

---

## Implementation

### Warning Categories

**Critical (Fix Immediately)**:
```
CS0618: Obsolete member usage (if no migration plan)
CS8600-CS8629: Nullable reference warnings (potential NullReferenceException)
ASP0019: Headers.Add() may throw on duplicate - use indexer syntax
TS2322: Type mismatch
TS2345: Argument type mismatch
```

**Important (Fix Before Merge)**:
```
CS0162: Unreachable code
CS0219: Variable assigned but never used
TS6133: Declared but never used
TS7006: Implicit 'any' type
```

**Minor (Fix When Convenient)**:
```
CS1591: Missing XML documentation
Performance suggestions
Code style warnings
```

### Acceptable Warnings

- **Obsolete Property Warnings**: If migration to new pattern is planned and documented
- **Nullable Reference Warnings**: If false positive (verified manually)
- **Performance Suggestions**: If not critical to current release

### Unacceptable Warnings

- ❌ Duplicate keys in object literals
- ❌ Unused variables or imports
- ❌ Unreachable code
- ❌ Type mismatches
- ❌ Missing await on async calls

---

## Common Fixes

### ASP0019: Headers.Add() Warning

```csharp
// ❌ FORBIDDEN (throws on duplicate keys)
context.Response.Headers.Add("X-Frame-Options", "DENY");

// ✅ CORRECT (idempotent, no exceptions)
context.Response.Headers["X-Frame-Options"] = "DENY";
```

### CS8600-CS8629: Nullable Reference

```csharp
// ❌ Warning: Possible null reference
string name = user.Name;

// ✅ Fixed: Null check or null-forgiving
string name = user.Name ?? "Unknown";
// or
string name = user.Name!;  // Only if verified non-null
```

### TS6133: Unused Variable

```typescript
// ❌ Warning: 'data' is declared but never used
const { data, error } = useQuery();

// ✅ Fixed: Use underscore prefix
const { data: _data, error } = useQuery();
// or remove if truly unused
const { error } = useQuery();
```

---

## Agent Behavior

### Before Commit

1. ✅ Review all warnings from build output
2. ✅ Categorize warnings (Critical/Important/Minor)
3. ✅ Fix all Critical warnings
4. ✅ Fix all Important warnings
5. ✅ Document Minor warnings in commit message

### Commit Message Format

```
feat: Add new feature

WARNINGS:
- CS1591: Missing XML docs (will add in next PR)
- Performance: Large bundle size (will optimize later)
```

### Reporting Format

```
⚠️ Build Warnings Summary:
- Critical: 0
- Important: 2 (unused variables - fixed)
- Minor: 5 (XML docs - deferred)

All critical and important warnings resolved.
Minor warnings documented in commit message.
```

---

## Suppression Rules

When suppression is necessary (rare):

```csharp
// Only suppress with documented reason
#pragma warning disable CS8618 // Non-nullable field not initialized
// Reason: Initialized by EF Core, not constructor
public string Name { get; set; }
#pragma warning restore CS8618
```

```typescript
// eslint-disable-next-line @typescript-eslint/no-unused-vars
// Reason: Required for type inference in generic context
const _placeholder = undefined;
```

---

## Anti-Patterns

❌ Ignoring warnings "because it works"
❌ Mass suppression without documentation
❌ Leaving Critical warnings unfixed
❌ Not categorizing warnings
❌ Committing with Important warnings

---

## Reference

- **Golden Rules**: Section 9
- **Build Hygiene**: [build-hygiene-standard.md](build-hygiene-standard.md)
- **Pre-Commit**: [pre-commit-standard.md](pre-commit-standard.md)
