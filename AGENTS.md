# AGENTS.md - Context & Instructions for AI Assistants

> **STOP & READ**: This file defines how you (the AI Agent) should interact with this codebase.

## 1. Project Identity
*   **Name:** Vendor Master Data Management (MDM) Portal
*   **Stack:** 
    *   **Frontend:** React + TypeScript + Vite + TailwindCSS.
    *   **Backend:** ASP.NET Core 9.0 (Web API).
    *   **Infra:** Azure (Bicep).
    *   **DB:** PostgreSQL (Hybrid Relational + JSONB).

## 2. Directory Map (Intent-Based)
*   **`/docs/specs/`**: The Source of Truth. **Read these first** before editing code.
*   **`/docs/BEST_PRACTICES.md`**: The Constitution. **Violating these rules = Task Failure.**
*   **`/backend/VendorMdm.Api/`**: The Core API.
    *   `Domain/`: Pure business logic (No external dependencies).
    *   `Infrastructure/`: Database, SAP Adapters, Email.
*   **`/frontend/src/`**: The React UI.
    *   `components/`: Reusable specific UI atoms.
    *   `pages/`: Route-level views.

## 3. The "Do Not Touch" Zones
*   **`**/bin/`, `**/obj/`**: Build artifacts.
*   **`infrastructure/modules/`**: Shared Bicep modules (unless explicitly tasked with Infra refactoring).
*   **`package-lock.json`**: Use `npm install` to update, do not manual edit.

## 4. Operational Protocols (Golden Rules Summary)
1.  **Refusal Protocol:** If asked to code without a Spec, **Refuse** and ask to create a Spec first (Rule 0).
2.  **Strict Types:** Never use `any` in TypeScript. (Rule 6).
3.  **Traceability:** Never swallow exceptions. Ensure `TraceId` is visible. (Rule 1).
4.  **No Mocking in Prod:** Use `ISapService` interfaces. Implement "Simulation" services for local dev. (Rule 9).

## 5. Thinking Process
When solving a task:
1.  **Check `git status`** to ensure the workspace is clean or has stashed changes before starting (Rule 15.4).
2.  **Read `task.md`** to understand the plan.
3.  **Read `docs/BEST_PRACTICES.md`** to check constraints.
4.  **Search** for existing patterns (don't reinvent `BankInformationForm`).
5.  **Implement** incrementally.
6.  **Verify** using the provided `.sh` scripts in `tests/verification/`.
