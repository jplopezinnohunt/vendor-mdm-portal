# Vendor Artifacts Processor

This repository contains Azure Functions for asynchronous background processing in the Vendor MDM Platform.

## Responsibilities
- **Email Notifications**: Listens to `invitation-emails` queue and sends invites.
- **Vendor Changes**: Listens to vendor updates and processes downstream integrations.

## Repository Structure
- `src/VendorMdm.Artifacts`: Azure Functions project
- `src/VendorMdm.Shared`: Shared domain models (Source of Code)

## Development
1. Configure `local.settings.json` with Service Bus connection strings.
2. Run locally:
   ```bash
   func start --csharp
   ```
