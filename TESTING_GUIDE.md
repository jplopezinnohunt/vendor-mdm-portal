# 🧪 Local Testing Guide for Vendor Artifacts

Since we are using a **Domain Artifact Architecture** with Azure Functions, testing locally requires configuring connections to either **Local Emulators** or **Azure Dev Resources**.

## 1. Prerequisites

Ensure you have the following installed:
- **.NET 8 SDK**
- **Azure Functions Core Tools** (`npm i -g azure-functions-core-tools@4`)
- **Azure Cosmos DB Emulator** (optional, for local NoSQL)
- **SQL Server Express / Docker** (optional, for local SQL)

## 2. Configure Local Settings

Open `backend/VendorMdm.Artifacts/local.settings.json`. You need to update the connection strings.

### Option A: Using Real Azure Dev Resources (Recommended)
If you have deployed the infrastructure (via `git push`), go to the Azure Portal:
1.  **SQL**: Get the connection string from the SQL Database.
2.  **Cosmos**: Get the Primary Connection String from the Cosmos DB Account.
3.  **Service Bus**: Get the "RootManageSharedAccessKey" connection string.

Update `local.settings.json`:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SqlConnectionString": "<YOUR_AZURE_SQL_CONNECTION_STRING>",
    "CosmosConnectionString": "<YOUR_AZURE_COSMOS_CONNECTION_STRING>",
    "ServiceBusConnection": "<YOUR_AZURE_SERVICE_BUS_CONNECTION_STRING>"
  }
}
```

### Option B: Using Local Emulators
If you want to run completely offline:

1.  **Cosmos DB Emulator**: Start it. Default connection is:
    `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;`
2.  **Local SQL**: Use your local connection string.
3.  **Service Bus**: *Note: Service Bus is hard to emulate locally. We recommend using a real Azure Service Bus namespace even for local dev.*

## 3. Run the Artifact Service

Open a terminal in `backend/VendorMdm.Artifacts/` and run:

```bash
dotnet build
func start
```

You should see the following endpoints startup:
- `[POST] http://localhost:7071/api/vendor/changerequest`
- `[GET]  http://localhost:7071/api/metadata/reference/{category}`
- `[GET]  http://localhost:7071/api/metadata/rules/{entityType}`
- ... and admin endpoints.

## 4. Test with cURL / Postman

### A. Create Reference Data (Admin)
```bash
curl -X POST http://localhost:7071/api/metadata/reference \
     -H "Content-Type: application/json" \
     -d '{ "category": "Country", "code": "US", "description": "United States", "isActive": true }'
```

### B. Submit a New Vendor
```bash
curl -X POST http://localhost:7071/api/vendor/changerequest \
     -H "Content-Type: application/json" \
     -d '{ "companyName": "Contoso Corp", "contactEmail": "admin@contoso.com" }'
```

### C. Verify Results
1.  **SQL**: Check the `ChangeRequests` and `VendorApplications` tables.
2.  **Cosmos**: Check the `ChangeRequestData` container for the JSON payload.
3.  **Cosmos**: Check the `DomainEvents` container for the `VendorApplicationSubmitted` event.
