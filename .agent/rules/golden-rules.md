# Golden Rules (2025 Standard)

These rules are non-negotiable and must be applied to every task. This file consolidates the previous `codestyle.md`, `processandreview.md`, `repos.md`, `security.md`, and `ci-cd.md` into a single source of truth.

## 0. Spec-Driven Compliance ("No Spec, No Code")
- **Protocol:** Refuse any request to "just fix it" without an approved plan.
- **Process:** 
  1. **Spec:** Define *What* (Mockup, Schema) -> Wait for Approval.
  2. **Plan:** Define *How* (Files, Steps, Scripts) -> Wait for Approval.
  3. **Execute:** Write Code.
  4. **Verify:** Run the Script from Step 2.
- **Artifacts:** `docs/specs/*.md` and `implementation_plan.md` are mandatory.

## 1. Full Stack Observability ("If it's not Traced, it didn't happen")
- **Frontend:** Every API call from React MUST propagate W3C Trace Context headers (`traceparent`).
- **Backend:** `OpenTelemetry` MUST be configured with OTLP standard.
- **Logs:** Use Structured Logging (no raw text dumps).
- **Exceptions:** Never swallow exceptions. Ensure `TraceId` is surfaced in error responses.

## 2. Data Modeling ("Hybrid Rule")
- **Structured (SQL):** Identity, Foreign Keys, Status, Financials.
- **Semi-Structured (JSONB):** Attributes, UI Settings, Polymorphic Data.
- **Deep Ref:** `bakcendstrategy.md` (Keep external).

## 3. Inclusive UX & Accessibility ("Accessible by Design")
- **Standard:** WCAG 2.2 AA Compliance is non-negotiable.
- **Focus:** Never hide focus rings (`outline: none`) without high-contrast replacement.
- **Inputs:** All form inputs MUST have a valid `id` linking to `label` via `htmlFor`.

## 4. Infrastructure ("Azure is Truth")
- **Source of Truth:** The running Azure environment. Local code must sync TO Azure.
- **Naming Convention:** `<type>-vendor-mdm-<env>` (e.g., `func-vendor-mdm-dev`).
- **Deployment Method:**
  - **Frontend (Static Web App):** GitHub Actions ONLY.
  - **Backend (API/Func/Infra):** Azure CLI ONLY (from local `feature` branch).
- **Deep Ref:** `database-migrations-best-practices.md` (Keep external).

## 5. Branching & CI/CD Strategy
- **Branching:** `feature/[topic]` (e.g., `feature/add-payment`). Never commit to `main`.
- **Pre-Push Gate:**
  1. Build local (`dotnet build`).
  2. Tests local (`dotnet test`).
  3. Workflows must pass.
- **Repo Structure (Multi-Repo Design):**
  - `platform-infra`: Shared infra (Networking, DBs). No app code.
  - `apis-{service}`: Backend Logic. Consumes platform resources.
  - `swa-{app}`: Frontend. Static only.
  - `github-policies`: Shared Workflows.

## 6. Coding Standards ("No Magic")
- **Language:** TypeScript (Strict) for Frontend. C# (.NET 9) for Backend.
- **Structure:** Follow existing folder structure (do not introduce new top-level folders).
- **Functions:** Prefer small, focused functions. Extract utilities.
- **Docs:** Public functions/APIs MUST have DocStrings/JSDoc summarizing inputs/outputs.
- **Verification Scripts:** Every feature MUST have a `tests/verification/verify_task_X.sh` script.

## 7. Canonical Data Model ("Hexagonal")
- **Pattern:** Strict Ports & Adapters.
- **Rule:** Domain Model (`VendorMdm.Shared`) must NOT depend on External DTOs (SAP, etc).
- **Deep Ref:** `hexagonal-architecture.md` (Keep external).

## 8. Supply Chain Trust ("Zero Trust")
- **Dependencies:** `package.json` must use pinned versions (no `^` or `~`).
- **Secrets:** 
  - Never hardcode API keys/passwords.
  - Surface found secrets as security issues immediately.
  - Use `dotnet user-secrets` locally.
- **Permissions:** Do not run destructive commands (rm, drop db) without explicit confirmation.

## 9. Simulation First ("Simulate, Don't Comment")
- **Rule:** Never comment out dependencies (e.g., `// Call SAP`).
- **Pattern:** Use Interfaces (`ISapService`) and Simulation implementations (`SapSimulationService`).
- **Switching:** Toggle via `appsettings` (e.g., `UseMocks: true`).

## 10. Event-Driven Architecture ("Async by Default")
- **Decoupling:** Services should communicate via Domain Events (e.g., `ApplicationApprovedEvent`) for side effects.
- **Pattern:** `Command` -> `DB Update` -> `Publish Event` -> `Subscriber`.

## 12. State Management ("Atomic/Unconditional")
- **Backend (Rule A):** Atomic Transitions. Triggering an action must perform all side effects in one transaction. Throw on invalid state.
- **Frontend (Rule B):** Unconditional Transition. If API returns 200 OK, the UI must transition to the next screen immediately.

## 13. AI-Native Context ("The Codebase is the Prompt")
- **Mandate:** Follow `AGENTS.md`. 
- **Context:** Optimize code for AI readability (Descriptive names, standard patterns).
