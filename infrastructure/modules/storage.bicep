// Azure Storage Account for Vendor Attachments
// Bicep Module - vendor-mdm-portal/infrastructure/modules/storage.bicep

@description('Environment name (dev, test, prod)')
param environmentName string

@description('Primary Azure region')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {}

// Storage Account Name (must be globally unique, lowercase, no hyphens)
var storageAccountName = 'st${replace('vendor-mdm', '-', '')}${environmentName}'

// Blob Container Names
var containerNames = [
  'vendor-attachments'
]

// ============================================================================
// Storage Account
// ============================================================================
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS' // Locally redundant storage (cheapest for dev)
  }
  kind: 'StorageV2' // General purpose v2
  properties: {
    accessTier: 'Hot' // Hot tier for frequently accessed data
    allowBlobPublicAccess: false // Disable public access for security
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      defaultAction: 'Allow' // Allow access from all networks (can restrict later)
      bypass: 'AzureServices'
    }
  }
}

// ============================================================================
// Blob Service with CORS Configuration
// ============================================================================
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            'https://*.azurestaticapps.net' // Allow SWA origins
            'http://localhost:5173' // Allow local dev
          ]
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
            'OPTIONS'
          ]
          allowedHeaders: [
            '*'
          ]
          exposedHeaders: [
            '*'
          ]
          maxAgeInSeconds: 3600
        }
      ]
    }
    deleteRetentionPolicy: {
      enabled: true
      days: 30 // Soft delete retention for 30 days
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

// ============================================================================
// Blob Containers
// ============================================================================
resource blobContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [for containerName in containerNames: {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'None' // Private container
  }
}]

// ============================================================================
// Outputs
// ============================================================================
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
