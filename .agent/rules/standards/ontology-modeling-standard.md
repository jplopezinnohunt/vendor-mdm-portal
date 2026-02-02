# Standard: Ontology Modeling & Definition

**Status:** ACTIVE
**Owner:** Architecture Team

---

## 1. Purpose
This standard defines how to model the "Ontology Layer"—the shared vocabulary and rules that exist independently of technical implementation (Database/API). All "Canonical Entities" must be defined in the Ontology before implementation.

## 2. Core Meta-Model
The Ontology consists of three primary components:

### A. Concepts (The Nouns)
*   **Definition**: A distinct thing in the business domain.
*   **Immutability**: A Concept's definition (What it *is*) does not change based on how it is created.
*   **Structure**: 
    *   **Identity**: What makes it unique (e.g., `TaxID`, `GlobalEventID`).
    *   **State**: The allowed lifecycle stages (e.g., `Draft`, `Active`, `Retired`).
    *   **Invariants**: Rules that are always true (e.g., "A Contract must have a Start Date").

### B. Relationships (The Verbs)
*   **Definition**: Named, directional links between concepts.
*   **Format**: `[Subject] --[PREDICATE]--> [Object]`
*   **Example**: `Contract --GOVERNS--> PurchaseOrder`.

### C. Origin Contexts (The Pathways)
*   **Definition**: The specific context or process that brought a Concept Instance into existence.
*   **Purpose**: To allow "same concept, different rules".
*   **Examples**:
    *   `Origin: DirectInvitation` (Requires strict TaxID).
    *   `Origin: EventOnboarding` (Allows provisional, sparse data).

---

## 3. Specification Requirements
When writing a `specs/spec_*.md` that involves data entities, you must include an **Ontology Alignment** section:

```markdown
## Ontology Alignment

### Affected Concepts
- **Vendor**: Extension. Adding `ClimateRating` attribute.
- **Project**: New Concept.

### Relationships
- `Project --FUNDS--> Contract` (1:N)

### Origin Contexts
- **GrantApplication**: New Flow. Created when a Vendor applies for a Grant.
    - *Constraint*: Vendor must be in `Active` state to start this flow.
```

## 4. The Registry
*   All valid Concepts are listed in `specs/ontology_registry.md`.
*   **Golden Rule**: If it's not in the Registry, it doesn't exist in the Ontology.
