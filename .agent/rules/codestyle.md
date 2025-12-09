---
trigger: always_on
---

Always use the primary language and stack already used in this repo (TypeScript for frontend/backend, Python or other languages only where already present). Prefer TypeScript with strict typing and avoid any except with an explicit comment explaining why it is safe.​

Follow the existing project structure and naming conventions. Do not introduce new top‑level folders or major architectural patterns without explicit approval.​

Match the existing formatting and linting setup. If tools like Prettier, ESLint, or other linters are configured, run them and fix reported issues before considering a task complete.​

Prefer small, focused functions and components. Extract shared logic into reusable utilities instead of duplicating code.​

When adding or changing public functions, components, or APIs, update or create docstrings / JSDoc / comments describing purpose, inputs, outputs, and important edge cases.​

For each non‑trivial feature or bugfix, add or update at least one automated test covering the main behavior, using the existing test framework and patterns in this project.​
