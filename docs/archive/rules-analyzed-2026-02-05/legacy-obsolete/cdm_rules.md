# Architectural Rule—Canonical Entities First

### **Policy Statement**

> Business entities are defined once, in the Canonical Data Model, and nowhere else.
All systems, integrations, and databases must adapt to these entities.
> 

---

## 🔹 ENTITY GOVERNANCE RULES (EXPLICIT)

### **Rule 1 — Single Source of Truth for Entities**

Each business entity (e.g. **Vendor, Customer, Employee, Project, Fund**) **must have exactly one canonical definition**.

- Defined in the Canonical Data Model (CDM)
- Versioned and documented
- Business-oriented naming only

❌ No duplicate entity definitions

❌ No SAP-named entities (e.g. LFA1, BUT000) outside adapters

---

### **Rule 2 — Canonical Entities Are the Only Public Contract**

Canonical entities are the **only models allowed** in:

- Public APIs
- Internal service APIs
- Frontend data contracts
- Events and messages

❌ SAP structures

❌ Database schemas

❌ Integration DTOs

→ **Never exposed outside adapters**

---

### **Rule 3 — Integration Models Are Entity Translations, Not Entities**

SAP, legacy, or external system models:

- Are **translations** of canonical entities
- Exist **only** inside adapter layers
- Must be fully replaceable

Example:

- `CanonicalVendor` ✅
- `SAP_LFA1` ❌ (outside adapter)

---

### **Rule 4 — Entity Ownership Belongs to the Domain**

The domain layer **owns**:

- Entity structure
- Field meaning
- Validation rules
- Lifecycle states

Integrations:

- Cannot add fields to entities
- Cannot rename entity attributes
- Cannot redefine entity semantics

---

### **Rule 5 — Persistence Does Not Define Entities**

Database schemas:

- Must conform to canonical entities
- Must not introduce new entity concepts
- Are implementation details

❌ “Table-driven domain”

✅ “Domain-driven persistence”

---

### **Rule 6 — Entity Changes Are Business-Driven Only**

A canonical entity may change **only if the business meaning changes**.

❌ Integration requirement

❌ SAP limitation

❌ Database optimization

→ These **must not** trigger entity changes.

---

### **Rule 7 — Entity Removal Test (Hard Rule)**

For every canonical entity:

> “If SAP is removed, does the entity still make sense and function?”
> 
- **No** → ❌ Architecture is invalid
- **Yes** → ✅ Correct design

---

## 🔹 ENTITY VALIDATION CHECK (FOR AGENTS)

Before implementing any entity-related change, the agent must answer:

1. Is this entity already defined canonically?
2. Does this change alter business meaning?
3. Does any SAP / DB concept leak into the entity?
4. Can the entity exist without SAP?

If **any answer is wrong** → reject the change.

---

## 🔹 NON-NEGOTIABLE STATEMENT

> Entities belong to the domain, not to SAP, not to the database, not to the cloud.
>