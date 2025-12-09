---
trigger: always_on
---

This workspace will have a multirepo desing 
You are working in a multi‑repo workspace for a cloud platform and its applications.
There are three main repos plus one governance repo:

platform-infra

Owns all shared cloud infrastructure and platform capabilities.

Contains IaC (Bicep/Terraform), environment compositions, shared resources (identity, event bus, databases, logging, networking), and reusable infra modules.

Does not contain app/business code.

When changing infra, keep modules generic and parameter‑driven so they can support multiple apps and tenants.

apis-{service} repos

Own API and backend services for specific domains or bounded contexts.

Consume platform resources provisioned by platform-infra (auth, event bus, databases, storage) rather than creating their own infra.

CI/CD for these repos should: build, test, and deploy app code, then bind to existing platform resources (via Key Vault, connection strings, environment variables, etc.).

Do not duplicate platform infra definitions here; if missing infra is needed, propose a new or extended module in platform-infra.

swa-{app} (Static Web Apps) repos

Own front‑end static web apps (e.g. portal, admin, public site).

Use APIs from apis-{service} and authentication/authorization from the platform.

Deployment workflows assume the Static Web App resource already exists (created by platform-infra) and only deploy static artifacts and config.

Front‑end code must treat API URLs, auth settings, and environment‑specific config as variables, not hard‑coded values.

github-policies repo

Central governance for all repos: shared GitHub Actions, reusable workflows, templates, and org conventions.

CI/CD workflows in other repos should, where possible, call reusable workflows from this repo (for build, test, lint, security checks, and common deploy patterns).

When suggesting new pipelines or policies, prefer implementing them once here, then referencing them from service/UI repos.

Global conventions
Keep platform concerns in platform-infra; keep business logic in apis-* and swa-*.

Prefer contracts over coupling: APIs communicate via HTTP/events with well-defined schemas; front‑ends depend on these contracts, not on infra details.

When adding a new service or app:

Add/extend infra modules in platform-infra if new shared capabilities are required.

Scaffold a new apis-{service} and/or swa-{app} repo using the standard templates and reusable workflows from github-policies.

Maintain clear separation of environments (dev/test/prod) and avoid environment‑specific logic in code when configuration can handle it.

How you should behave
When asked for changes, first identify which repo is the correct place, then propose edits only in that repo.

Prefer designs and changes that:

Reuse platform modules rather than duplicating resources.

Keep CI/CD simple by reusing shared workflows.

Preserve independence of app/API repos while keeping them aligned to the platform contracts and policies.

When something is ambiguous (for example, infra vs app responsibility), propose the cleanest separation that keeps the platform reusable across multiple apps.

You can tweak repo names (e.g. vendor-platform-infra, vendor-apis, vendor-portal-swa) and add any Azure‑specific details (Container Apps, Static Web Apps, Service Bus, etc.) directly into the text before pasting it into your rules file


