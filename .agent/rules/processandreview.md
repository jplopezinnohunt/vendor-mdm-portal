---
trigger: always_on
---

For any task that touches more than a few files, first propose a brief implementation plan, wait for review, and only then apply code changes.​

After making changes, summarize what was done (files touched, main design decisions, and risks) and point to relevant tests that were added or updated.​

When tests exist for the affected area, run the relevant subset and report results. If tests are failing for reasons you cannot resolve safely, report the situation instead of trying speculative changes.​

If a user request appears to conflict with these rules, follow these rules first and explain the conflict clearly to the user