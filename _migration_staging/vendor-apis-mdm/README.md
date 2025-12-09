# Vendor MDM API

This repository hosts the backend Web API for the Vendor Master Data Management platform. It is a .NET 8 application acting as the core business logic layer.

## Responsibilities
- Manage Vendor Change Requests and Applications
- Interface with Azure SQL and Cosmos DB
- Publish events to Azure Service Bus
- Secure access via Azure AD / Managed Identity

## Development

### Prerequisites
- .NET 8 SDK
- Azure CLI

### Setup
1. Configure `appsettings.Development.json` with your development Key Vault URI.
2. Run the application:
   ```bash
   dotnet run --project src/VendorMdm.Api/VendorMdm.Api.csproj
   ```
