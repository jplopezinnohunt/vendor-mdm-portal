# Vendor Platform Infrastructure

This repository contains the shared cloud infrastructure for the Vendor MDM Platform. It uses Azure Bicep to provision and manage resources such as:

- **Data**: Azure Cosmos DB, Azure SQL Database
- **Messaging**: Azure Service Bus
- **Compute**: Azure Functions (Infrastructure only), Azure Static Web Apps (Infrastructure only)
- **Security**: Key Vault, Managed Identities
- **Monitoring**: Application Insights

## Repository Structure

```
.
├── .github/workflows/    # CI/CD pipelines
├── bicep/               # Infrastructure as Code
│   ├── main.bicep       # Main orchestration
│   ├── modules/         # Reusable modules
│   └── environments/    # Environment-specific parameters
└── scripts/             # Utility scripts
```

## Deployment

Deployments are managed via GitHub Actions.

### Manual Deployment
```bash
az deployment group create \
  --resource-group <rg-name> \
  --template-file bicep/main.bicep \
  --parameters bicep/environments/dev.bicepparam
```
