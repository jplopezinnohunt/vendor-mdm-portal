# Verification Scripts

This directory contains automated verifications scripts for Spec-Driven Development tasks.

## Naming Convention
`verify_task_[task_id]_[description].[sh|ts|js|py]`

## Requirement
Every script must be:
1. **Executable**: `chmod +x`
2. **Idempotent**: Can be run multiple times without side effects (cleanup after self if needed).
3. **Boolean**: Exit code 0 for Success, non-zero for Failure.

## Examples
- `verify_task_005_fix_login.sh`: Curling the endpoint to check for 200 OK.
- `verify_task_006_invite_flow.spec.ts`: Playwright test for the invitation UI.
