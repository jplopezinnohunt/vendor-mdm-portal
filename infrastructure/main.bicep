// Main Bicep template for Vendor MDM Portal
// Optimized for Azure Free Tier & Dev/Test purposes

@description('Environment name (dev, test, prod)')
@allowed(['dev', 'test', 'prod'])
param environmentName string = 'dev'

@description('Primary Azure region for resources')
param location string = resourceGroup().location

@description('Admin username for SQL Server - OPTIONAL (SQL deployment disabled)')
@secure()
param sqlAdminUsername string = ''

@description('Admin password for SQL Server - OPTIONAL (SQL deployment disabled)')
@secure()
param sqlAdminPassword string = ''

// Azure AD Authentication Parameters
// Currently DISABLED for API testing (see PENDING-AUTH-REENABLE.md)
// Uncomment lines below when re-enabling authentication
@description('Azure AD Tenant ID - Required for authentication')
param azureAdTenantId string = ''

@description('Azure AD Client ID (Application ID) - Required for authentication')
param azureAdClientId string = ''

@description('Your company name for email templates')
param companyName string = 'Your Company'

// Naming convention - clean and readable
var prefix = 'vendor-mdm'
var suffix = environmentName

// Resource names (no hash suffix for clarity)
var sqlServerName = 'sql-${prefix}-${suffix}'
var sqlDatabaseName = 'VendorMdmDb'
var cosmosAccountName = 'cosmos-${prefix}-${suffix}'
var serviceBusNamespaceName = 'sb-${prefix}-${suffix}'
var keyVaultName = 'kv-${prefix}-${suffix}'
var appServicePlanName = 'asp-${prefix}-${suffix}'
var appServiceName = 'app-${prefix}-api-${suffix}'
var staticWebAppName = 'swa-${prefix}-${suffix}'
var appInsightsName = 'ai-${prefix}-${suffix}'
var storageAccountName = 'st${replace(prefix, '-', '')}${suffix}' // Storage account name (no hyphens)


// ============================================================================
// 1. Azure SQL Database (Basic - 5 DTU - Cheapest)
// ============================================================================
// TEMPORARILY DISABLED - SQL Server already exists, avoid parameter conflicts
/*
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic' // Basic Tier (lowest cost)
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    requestedBackupStorageRedundancy: 'Local'
  }
}

// Allow Azure services to access SQL Server
resource sqlFirewallRuleAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}
*/

// ============================================================================
// 2. Cosmos DB (Serverless - Pay per request)
// ============================================================================
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless' // Serverless is best for dev/test
      }
    ]
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: 'VendorMdm'
  properties: {
    resource: {
      id: 'VendorMdm'
    }
  }
}

resource cosmosContainerArtifacts 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'InvitationArtifacts'
  properties: {
    resource: {
      id: 'InvitationArtifacts'
      partitionKey: {
        paths: ['/InvitationId']
        kind: 'Hash'
      }
    }
  }
}

resource cosmosContainerEvents 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'DomainEvents'
  properties: {
    resource: {
      id: 'DomainEvents'
      partitionKey: {
        paths: ['/EventType']
        kind: 'Hash'
      }
    }
  }
}

// ============================================================================
// 3. Service Bus (Basic Tier)
// ============================================================================
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Basic' // Basic Tier is cheaper, supports Queues (not Topics)
    tier: 'Basic'
  }
}

// Note: Basic Tier only supports Queues, not Topics.
// We'll use a Queue instead of Topic for dev environment
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'invitation-created'
  properties: {
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P14D'
  }
}

// ============================================================================
// 4. Azure Storage Account (for vendor attachments)
// ============================================================================
module storageModule './modules/storage.bicep' = {
  name: 'storage-deployment'
  params: {
    environmentName: environmentName
    location: location
    tags: {
      Environment: environmentName
      Project: 'VendorMDM'
      Component: 'Storage'
    }
  }
}

// ============================================================================
// 5. Key Vault (Standard)
// ============================================================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: false
    accessPolicies: []
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// Store connection strings in Key Vault
// SQL Connection - DISABLED (server already exists)
/*
resource secretSqlConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--Sql'
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}
*/

resource secretCosmosConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--Cosmos'
  properties: {
    value: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
  }
}

resource secretServiceBusConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--ServiceBus'
  properties: {
    value: listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
  }
}

resource secretBlobStorageConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--BlobStorage'
  properties: {
    value: storageModule.outputs.connectionString
  }
}

// ============================================================================
// 6. App Service Plan & Backend API (Free Tier F1)
// ============================================================================

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1' // Free Tier
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: false // Free tier doesn't support AlwaysOn
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'UseLocalEmulators'
          value: 'false'
        }
        {
          name: 'KeyVault__VaultUrl'
          value: keyVault.properties.vaultUri
        }
        // REMOVED: Azure AD configuration - authentication disabled for testing
        // Uncomment when re-enabling auth (see PENDING-AUTH-REENABLE.md)
        // {
        //   name: 'AzureAd__ClientId'
        //   value: azureAdClientId
        // }
        // {
        //   name: 'AzureAd__TenantId'
        //   value: azureAdTenantId
        // }
        // {
        //   name: 'AzureAd__Domain'
        //   value: '${azureAdTenantId}.onmicrosoft.com'
        // }
        {
          name: 'App__CompanyName'
          value: companyName
        }
        {
          name: 'App__BaseUrl'
          value: 'https://${staticWebAppName}.azurestaticapps.net'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
  }
}

// Grant App Service access to Key Vault
resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: appService.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
  }
}

// RBAC: Grant App Service managed identity access to Storage Account
// TODO: Add this manually after deployment or in separate deployment
// resource appServiceStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(appService.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe', 'storage-blob-contributor')
//   scope: resourceGroup()
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
//     principalId: appService.identity.principalId
//     principalType: 'ServicePrincipal'
//   }
//   dependsOn: [
//     storageModule
//     appService
//   ]
// }

// ============================================================================
// 6. Static Web App (Frontend - Free Tier)
// ============================================================================

resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: location // Consolidate to Central US with other resources
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    // repositoryUrl: '' - Removed to allow manual deployment via token
    // No repositoryUrl or buildProperties needed for detached local deployment
  }
}

// Configure Static Web App settings
resource staticWebAppSettings 'Microsoft.Web/staticSites/config@2023-01-01' = {
  parent: staticWebApp
  name: 'appsettings'
  properties: {
    VITE_API_BASE_URL: 'https://${appService.properties.defaultHostName}/api'
  }
}

// ============================================================================
// 7. Application Insights
// ============================================================================
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

// ============================================================================
// Outputs
// ============================================================================
output resourceGroupName string = resourceGroup().name
// SQL outputs - DISABLED (server deployment skipped)
// output sqlServerName string = sqlServer.name
// output sqlDatabaseName string = sqlDatabase.name
output cosmosAccountName string = cosmosAccount.name
output serviceBusNamespace string = serviceBusNamespace.name
output keyVaultName string = keyVault.name
output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output staticWebAppName string = staticWebApp.name
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
output staticWebAppDeploymentToken string = listSecrets(staticWebApp.id, staticWebApp.apiVersion).properties.apiKey
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output storageAccountName string = storageModule.outputs.storageAccountName
output blobEndpoint string = storageModule.outputs.blobEndpoint

