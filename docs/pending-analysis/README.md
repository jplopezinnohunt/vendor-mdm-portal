# Pending Analysis - Documentation Queue

Files organized by topic, awaiting review to determine if content should be:
- **Integrated** into Golden Rules or Standards
- **Kept** as reference documentation
- **Archived** as obsolete

## Folders

| Folder | Content | Priority |
|--------|---------|----------|
| `rules/` | Potential rule documents | High |
| `architecture/` | Architecture designs | Medium |
| `diagrams/` | Diagram generation guides | Low |
| `implementation/` | Implementation plans/summaries | Medium |
| `integration/` | SAP, Sanctions, external integrations | Medium |
| `legacy-root/` | Files from project root | High |

## Analysis Process

1. Read each file
2. Evaluate against Golden Rules
3. Decision:
   - **Integrate**: Add value to golden rules → Update moderngoldenrules.md
   - **Keep**: Reference documentation → Move to appropriate docs/ folder
   - **Archive**: Obsolete content → Move to docs/archive/

## Files Awaiting Analysis

### rules/ (2 files)
- `git-workflow-best-practices.md` - May have rules not in golden rules
- `DATABASE_SCHEMA.md` - Schema documentation

### architecture/ (5 files)
- `architecture_design.md`
- `ARCHITECTURE_DETAILED.md`
- `progressive-integration-architecture.md`
- `file-storage-service-architecture.md`
- `complete-service-integration-map.md`

### diagrams/ (11 files)
- Various diagram generation and viewing guides
- Likely can be consolidated or archived

### implementation/ (7 files)
- Migration guides
- Implementation summaries
- Deployment plans

### integration/ (4 files)
- SAP simulation docs
- Sanctions screening plans

### legacy-root/ (8 files)
- `cdm_rules.md` - May have rules to extract
- `AGENTS.md` - Agent instructions
- `HANDOVER.md` - Handover documentation
- Implementation plans
- Walkthroughs
