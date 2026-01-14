# Project Best Practices & Golden Rules
**"The Constitution of the Codebase"**

This document acts as the **Root Authority** for all development. It defines the immutable principles.
*   **For UI Details:** See [UI Design Standards](./ui_design_standards.md) (Enforced by Rule 3).
*   **For Feature Specs:** See individual `.md` files in `docs/specs/` (Enforced by Rule 0).
*   **For AI Agents:** See [AGENTS.md](./AGENTS.md) (Enforced by Rule 13).

## 0. Enforceability
**"No Spec without Compliance"**
*   **The Check**: Every `spec_*.md` file MUST cite the specific rules from this document that apply to the task.
*   **The Protocol**: Use the **Refusal Protocol**. If asked to "just fix it" without a Spec/Plan, the Agent/Developer must say:
    > *"I cannot proceed to Execution. We must first define the Specification and Verification Plan as per the project rules."*

## 1. Full Stack Observability (Trace & visualize)
**"If it's not Traced, it didn't happen."**

*   **Rationale:** "Visible Debugging" (Console Overlay) is good for local UI, but insufficient for distributed cloud systems.
*   **The Rule:**
    1.  **Frontend-to-Backend Tracing:** Every API call from the React Frontend MUST propagate W3C Trace Context headers (`traceparent`).
    2.  **OpenTelemetry Standard:** Use OTLP (OpenTelemetry Protocol) for all telemetry.
    3.  **Structured Logging:** Log with standard attributes (`user.id`, `tenant.id`, `request.type`). No unstructured text dumps.
    4.  **Local Debug Overlay:** The Frontend MUST still render critical errors (`window.onerror`) in a Red Box Overlay for visual agents, but MUST also display the `TraceId` for correlation.

## 2. Data Modeling: The Hybrid Rule
**"Structured Identity, Semi-Structured Attributes."**

| Data Type | Sorage | Why? |
| :--- | :--- | :--- |
| **Core Identity** (Keys, Foreign Keys, Status, specific Money) | **SQL Column** | Integrity, Indexing, Joins. |
| **Attributes** (Settings, UI Config, Sparse Data, Logs) | **JSONB** | Flexibility, Schema Evolution. |

*   **Anti-Pattern:** Adding a column `preferred_color` to the root table. (Put it in `attributes` JSONB).

## 3. Inclusive UX & Accessibility
**"Accessible by Design, Not Audit"**
*   **Full Standard:** [docs/ui_design_standards.md](./ui_design_standards.md)
*   **The Golden Rule:** WCAG 2.2 AA Compliance is non-negotiable.
*   **Key Principles:**
    1.  **Focus Visibility:** "Focus Not Obscured". Custom focus rings must be high-contrast and never hidden.
    2.  **Target Size:** Minimum 24x24px clickable area for all interactive elements (Mobile/Touch ready).
    3.  **Cognitive Load:** Use "Consistent Help" patterns. Error messages must be descriptive.
    4.  **Aesthetics:** Premium feel is a functional requirement (Rule 3.4).

## 4. Infrastructure: Azure is Truth
*   **Source of Truth:** The running Azure environment is the master.
*   **Deployment:**
    *   Frontend: GitHub Actions.
    *   Backend/Infra: Azure CLI (local execution).
*   **Validation:** Always run `az bicep build` and compare with current state before deploying.

## 5. Development Workflow: Spec-Driven
**"Measure Twice, Cut Once."**

1.  **Spec:** Define *What* (Mockup, Schema, Acceptance) -> **Wait for Approval**.
2.  **Plan:** Define *How* (Files, Steps, Verification Script) -> **Wait for Approval**.
3.  **Execute:** Write Code.
4.  **Verify:** Run the Script from Step 2.

## 6. Coding Standards
*   **Verification Scripts:** Every feature must have a `tests/verification/verify_task_X.sh` script.
*   **No Magic Strings:** Use constants or Enums.
*   **Strict Types:** No `any` in TypeScript.
*   **Interface Integrity:** When modifying an interface (e.g., adding `TestConnectionAsync`), perform a global search to ensure ALL implementations (Real, Mock, Simulation, etc.) are updated. Never leave "stale" implementations.

## 7. Canonical Data Model (CDM)
**"The Hexagonal Standard"**
*   **Concept:** We use a Canonical Data Model to decouple the App Layer from External Integrations (SAP, Salesforce, etc.).
*   **Rule:** Never use external DTOs (e.g., `SapVendorDto`) directly in the Core Domain or UI.
*   **Flow:** `External API` -> `Adapter (Map to CDM)` -> `Core Service` -> `Database`.
*   **Storage:** The database schema reflects the *CDM*, not the external system's schema.

## 8. Security & Supply Chain Trust
**"Zero Trust & Verify Source"**
*   **Source of Truth:** Azure Active Directory (Entra ID). No local user tables.
*   **Supply Chain:**
    *   **SBOM:** Every build artifact MUST generate a Software Bill of Materials.
    *   **Lockfiles:** `package-lock.json` MUST be committed and immutable in CI.
    *   **Strict Dependencies:** No `latest` tags. Pin specific versions.
*   **Secrets:**
    *   **Local:** User Secrets (`dotnet user-secrets`), never `appsettings.json`.
    *   **Azure:** Key Vault Managed Identity.
    *   **Frontend:** Never store secrets in React.

## 9. Simulation First (No Hardcoding)
**"Simulate Behavior, Don't Comment Out"**
*   **The Problem:** Commented-out code blocks (e.g., `// TODO: Call SAP`) or hardcoded returns (`return true;`) create tech debt and untestable holes in the workflow.
*   **The Rule:** If a dependency (e.g., SAP, Email) is not ready:
    1.  Define the **Interface** (e.g., `ISapService`).
    2.  Create a **Simulation Service** (e.g., `SapSimulationService`) that implements realistic behavior (logs actions, returns success/failure objects, mimics latency).
    3.  Write the **Consumer Code** fully, as if the real service exists.
*   **Simulation Transparency:** Every simulation MUST explicitly log its actions with a distinct prefix: `[SIMULATION MODE - NO EXTERNAL ACTION]`. This prevents developers from investigating "missing" external side effects when in simulation mode.
*   **Switching:** Use `appsettings.UseMocks: true` to inject the simulation.
*   **Result:** The application flow is fully functional and testable from Day 1.

## 10. Global Event-Driven Architecture
**"Async by Design, Sync by Necessity"**
*   **Scope:** This is the default architectural pattern for the **Entire Solution**. Services communicate via Domain Events, not tight coupling.
*   **The Pattern:**
    1.  **Command:** API handles User Intent -> Updates own DB -> Publishes **Event** (e.g., `VendorSubmitted`, `UserInvited`).
    2.  **Reaction:** Other Services (SAP Worker, Notification Service, Audit Log) subscribe to the Event and act.
*   **Key Benefit:** Decouples the Portal from the latency and availability of downstream systems (SAP, Email, etc.).
*   **SAP Example:** SAP integration is just one *Subscriber* in this global architecture, satisfying the "System of Record" requirement asynchronously.

## 11. CI/CD & Branching Strategy
**"Verified Locally, Deployed Authorization"**
*   **Branching Rule:** **New Conversation = New Feature Branch**.
    *   Naming: `feature/[conversation-topic]` (e.g., `feature/add-payment-gateway`).
    *   Never commit directly to `develop` or `main`.
*   **Flow:** `feature/*` -> Pull Request -> `develop` -> `main`.
*   **Deployment:**
    *   **Frontend:** Auto-deploy via GitHub Actions on merge.
    *   **Backend:** Manual Azure CLI deploy from local `feature` branch for verification, then merge.

## 12. State Management: Atomic, Strict, & Unconditional
**"Robustness via Separation of Concerns"**
*   **Concept:** The Backend owns the *State Machine*. The Frontend owns the *User Experience*.
*   **Rule A (Backend): Atomic Transitions**:
    *   Triggering an action (e.g. `SubmitEnrichment`) MUST atomically perform all side effects (e.g. `CreateApplication`) in one transaction.
    *   **Strict Enforcement:** The Backend MUST throw an error if a transition is attempted from an invalid state (e.g., Approving a "Draft"). Never rely solely on the UI to hide buttons.
*   **Rule B (Frontend): Unconditional Transitions**:
    *   If an API call returns `200 OK`, the UI MUST unconditionally transition to the next screen (e.g., Dashboard).
    *   *Why?* Because Rule A guarantees the state is valid. The UI does not need to double-check.
*   **Rule C (Verification): The "Transition Code Check"**:
    *   Every complex state transition MUST be verified by a dedicated `tests/verification/verify_transition_X.sh` script.
    *   This script acts as the "Code Check" logic gate before release.

## 13. AI-Native Context
**"The Codebase is the Prompt"**
*   **Rationale:** AI agents co-develop this system. The code structure must be optimized for machine understanding.
*   **The Rule:**
    1.  **`AGENTS.md` is Mandatory:** A root-level file explicitly instructing agents on project structure, conventions, and "Do Not Touches".
    2.  **Context Isolation:** Prefer "Vertical Slice" architecture to minimize tokens required to understand a feature.
    3.  **Semantic Naming:** Directory/file names must describe *intent* (e.g., `features/onboarding/`) not just type.
    4.  **Self-Documentation:** Complex logic must include "Why" comments targeting AI reasoning.

## 14. Dependency Health Awareness
**"Know thy Neighbors"**
*   **The Problem:** Systems often fail silently when external dependencies (Email, SAP, Storage) are down, leading to fragmented state and poor user experience.
*   **The Rule:**
    1.  **Connectivity Probes:** Every external service client MUST implement a `TestConnectionAsync` method that performs a real connectivity check (ping, OPTIONS request, or port connection).
    2.  **Health Expose:** Proactive connectivity status MUST be exposed via the `/api/system/data-sources` or `/api/health` endpoints.
    3.  **UI Fail-Fast:** The Frontend MUST query these statuses and proactively warn the user *before* they initiate a workflow that depends on a failing service.
    4.  **Truth in Success:** No "Silent Masking". If an external call fails in production, the service MUST NOT return a `Success: true` flag even if it successfully logged the failure. Real-world intent (e.g., email sent) must match the returned status.
    5.  **Contextual Error Logs:** Critical failures in production MUST log the current configuration state (e.g., `SMTP Enabled: false`, `URL: ...`) alongside the exception to aid instant diagnosis without needing to re-check config files.
