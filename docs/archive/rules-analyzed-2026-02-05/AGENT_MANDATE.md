---
trigger: always_on
---

# 🤖 AGENT MANDATE: Spec-Driven Development

**CRITICAL INSTRUCTION**: You (the Agent) are required to follow this workflow for every task.

## 1. The Workflow
You MUST use the executable workflow located at:
`/.agent/workflows/spec-driven-dev.md`

## 2. The Refusal Protocol
If a user request lacks a Specification (Spec), you MUST:
1.  **Stop Execution.**
2.  **Reply:** *"I cannot proceed to Execution. We must first define the Specification and Verification Plan as per the project rules."*
3.  **Enter Planning Mode** to help the user draft the Spec.

## 3. The Golden Rules (Summary)
You MUST cross-reference every plan against these critical standards.

**0. Enforceability:** "No Spec without Compliance."
**1. Data Modeling:** Hybrid Model. (Core = SQL, Attributes = JSONB).
**2. UI Principles:** Uniformity, Proximity, Feedback, Aesthetics. (Reference `docs/ui_design_standards.md`).
**3. Infrastructure:** "Azure is Truth." (Manual Backend Deploy, Auto Frontend).
**4. Workflow:** Spec -> Plan -> Execute -> Verify.
**5. Code Standards:** No Magic Strings, Strict Types, and Mandatory Verification Scripts.
**6. Canonical Data Model:** Decouple App from Integrations via Adapters.
**7. Security:** Zero Trust. Azure AD for Auth. RBAC at API.
**8. Simulation:** "Work Offline." Mock First for all external services.
**9. EDA (Event-Driven):** Global Async. Use Domain Events for decoupling.
**10. CI/CD:** Feature Branch per Conversation (`feature/[topic]`). Merge to `develop`.

*Full Details:* `docs/BEST_PRACTICES.md`

## 4. Verification
You CANNOT consider a task complete until you have:
1.  Created a `tests/verification/verify_task_X.sh` script.
2.  Executed it successfully.
3.  Produced a `walkthrough.md` with proof.
