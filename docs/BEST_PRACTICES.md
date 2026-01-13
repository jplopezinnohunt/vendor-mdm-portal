# Project Best Practices & Golden Rules

## 0. Enforceability
**"No Spec without Compliance"**
*   **The Check**: Every `spec_*.md` file MUST cite the specific rules from this document that apply to the task.
*   **The Protocol**: Use the **Refusal Protocol**. If asked to "just fix it" without a Spec/Plan, the Agent/Developer must say:
    > *"I cannot proceed to Execution. We must first define the Specification and Verification Plan as per the project rules."*

## 1. Visible Debugging (Console-to-UI)
**The Concept:**
AI Agents and visual testing tools cannot open Chrome DevTools. If an error occurs that is only printed to the console, the agent is blind to it.

**The Rule:**
All frontend applications MUST include a **Debug Console Overlay** in Development/Test environments.
- **Requirement:** Catch `window.onerror`, `unhandledrejection`, and `console.error`.
- **Display:** Render these errors in a high z-index, distinct visual container (e.g., Red Box) on top of the UI.
- **Goal:** Ensure any crash produces a **visible artifact** in screenshots/videos, allowing the agent to self-diagnose failures immediately.
- **In Azure/Prod:** Must be activatable via URL parameter (e.g., `?debug=true`) so the agent can debug deployed environments without exposing errors to public users.

## 2. Data Modeling: The Hybrid Rule
**"Structured Identity, Semi-Structured Attributes."**

| Data Type | Sorage | Why? |
| :--- | :--- | :--- |
| **Core Identity** (Keys, Foreign Keys, Status, specific Money) | **SQL Column** | Integrity, Indexing, Joins. |
| **Attributes** (Settings, UI Config, Sparse Data, Logs) | **JSONB** | Flexibility, Schema Evolution. |

*   **Anti-Pattern:** Adding a column `preferred_color` to the root table. (Put it in `attributes` JSONB).

## 3. Principle-Driven UI Implementation
**"Consistency is Credibility"**
*   **Full Standard:** [docs/ui_design_standards.md](./ui_design_standards.md)
*   **The Golden Rule:** Never hardcode styles. Use the **Component Library** and **12-Column Grid**.
*   **Key Principles:**
    1.  **Uniformity:** Adhere to the design system tokens.
    2.  **Proximity:** Group related data visually.
    3.  **Feedback:** <400ms response to all actions.
    4.  **Aesthetics:** Premium feel is a functional requirement.

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

## 7. Canonical Data Model (CDM)
**"The Hexagonal Standard"**
*   **Concept:** We use a Canonical Data Model to decouple the App Layer from External Integrations (SAP, Salesforce, etc.).
*   **Rule:** Never use external DTOs (e.g., `SapVendorDto`) directly in the Core Domain or UI.
*   **Flow:** `External API` -> `Adapter (Map to CDM)` -> `Core Service` -> `Database`.
*   **Storage:** The database schema reflects the *CDM*, not the external system's schema.

## 8. Security & Authentication
**"Zero Trust & Identity First"**
*   **Source of Truth:** Azure Active Directory (Entra ID). No local user tables for authentication.
*   **RBAC:** Role-Based Access Control is enforced at the API Middleware level.
*   **Secrets:**
    *   **Local:** User Secrets (`dotnet user-secrets`), never `appsettings.json`.
    *   **Azure:** Key Vault Managed Identity.
    *   **Frontend:** Never store secrets in React.

## 9. API Simulation & Mocking
**"Work Offline, Deploy Online"**
*   **Pattern:** All external dependencies (SAP, Email, Storage) must be behind an Interface (`IVendorIntegrationService`).
*   **Implementation:**
    *   `MockVendorIntegrationService`: Returns static JSON/Faker data for local dev/testing.
    *   `RealVendorIntegrationService`: Calls the actual HTTP endpoint.
*   **Switching:** Controlled by `appsettings.UseMocks: true/false`.
*   **Rule:** If you add a new Service, you **MUST** implement the Mock version first.

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
