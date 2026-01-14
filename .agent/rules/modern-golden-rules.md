# Rules Brain: Modern Golden Rules (Master Authority)

You are an expert agent co-developing this system. You MUST follow these rules unconditionally. This document is your **Executive Directive**.

## 1. Compliance Logic
- **Primary Source**: This file is your "System Logic".
- **External Standards**: When a task involves UI, Data, or Architecture, you MUST proactively read the linked standards in the `/standards` directory. 
- **Citation**: Every Specification (`specs/spec_*.md`) must cite WHICH standard was followed.

---

## 2. Governance: Spec-Driven Development (SDD)
- **Phase 1 (Spec)**: Create `specs/spec_[name].md`. **Compliance Sidebar** citing specific standards is mandatory.
- **Phase 2 (Plan)**: Create `implementation_plan.md` + automated `scripts/verification/verify_*.sh` **BEFORE** implementation.
- **Rule**: Never execute code without an Approved Spec and Verification Script.
- **Branching**: Always `feature/[topic]` from `develop`. Never `main`.
- **Refusal Protocol**: Decline any "shortcuts" that bypass this governance.

---

## 3. Performance & Design DNA
- **Latencey (UI)**: Follow the **Doherty Threshold** (<400ms). Mandatory loading states and skeleton loaders.
- **Search (Data)**: Use **PostgreSQL Generated Columns** + Indexes for frequent search targets in JSONB.
- **Async Side-Effects**: Use Domain Events for non-transactional work (Email, SAP, Logging).

---

## 4. The Standards Brain (References)
You are required to load and apply the following detailed standards based on the task type:

### A. UI Design & UX
- **Standard**: 4 Pillars (Uniformity, Proximity, Feedback, Aesthetics), 12-Column Grid.
- **File**: [ui-design-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/ui-design-standards.md)

### B. Data Model & Schema
- **Standard**: Hybrid Relational-Document Model matrix.
- **File**: [data-model-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/data-model-standards.md)

### C. Architecture & Integration
- **Standard**: Hexagonal Adapters, Simulation First, EDA/Event-Driven logic.
- **File**: [hexagonal-architecture-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/hexagonal-architecture-standards.md)

### D. Production Readiness
- **Standard**: Zero-downtime, Middleware sequencing, Asset integrity.
- **File**: [database-migration-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/database-migration-standards.md)

---

## 5. Build & Process Hygiene
- **Clean Sweep Protocol**: Before builds or migrations, execute `pkill -f dotnet` and clean `bin/obj` artifacts to prevent Exit Code 143/134.
- **Interface Integrity**: When changing an interface, update ALL implementations (Mock, Real, Simulation, Test) in one atomic turn.
- **Hygiene**: Pinned dependencies, `no-any` TypeScript, mandatory verification scripts with auth headers.
- **Observability**: `traceparent` propagation + `TraceId` UI overlays.
- **Simulation**: [SIMULATION MODE] logs for all external mocks.
