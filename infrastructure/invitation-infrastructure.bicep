// Bicep template for Invitation-Based Onboarding Azure Infrastructure
// Deploy with: az deployment group create --resource-group <rg-name> --template-file invitation-infrastructure.bicep

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Base name for all resources')
param baseName string = 'vendormdm'

// Variables
var serviceBusNamespaceName = '${baseName}-sb-${environment}'
var invitationQueueName = 'invitation-emails'
var vendorChangesQueueName = 'vendor-changes'
var functionAppName = '${baseName}-func-${environment}'
var storageAccountName = '${baseName}st${environment}${uniqueString(resourceGroup().id)}'
var appInsightsName = '${baseName}-ai-${environment}'

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    zoneRedundant: false
  }
}

// Service Bus Queue: invitation-emails
resource invitationEmailQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  parent: serviceBusNamespace
  name: invitationQueueName
  properties: {
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P14D' // 14 days
    lockDuration: 'PT5M' // 5 minutes
    maxDeliveryCount: 10
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    deadLetteringOnMessageExpiration: true
    enablePartitioning: false
  }
}

// Service Bus Queue: vendor-changes (existing/default queue)
resource vendorChangesQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  parent: serviceBusNamespace
  name: vendorChangesQueueName
  properties: {
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P14D'
    lockDuration: 'PT5M'
    maxDeliveryCount: 10
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    deadLetteringOnMessageExpiration: true
    enablePartitioning: false
  }
}

// Storage Account for Azure Functions
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// App Service Plan for Functions
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${baseName}-asp-${environment}'
  location: location
  sku: {
    name: environment == 'prod' ? 'EP1' : 'Y1' // Premium for prod, Consumption for dev/staging
    tier: environment == 'prod' ? 'ElasticPremium' : 'Dynamic'
  }
  properties: {
    reserved: false
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ServiceBusConnection'
          value: listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
        }
        {
          name: 'APP_BASE_URL'
          value: environment == 'prod' ? 'https://vendor-portal.yourcompany.com' : 'https://${baseName}-${environment}.azurewebsites.net'
        }
        {
          name: 'COMPANY_NAME'
          value: 'Your Company Name'
        }
        {
          name: 'SUPPORT_EMAIL'
          value: 'vendorsupport@yourcompany.com'
        }
        {
          name: 'SUPPORT_PHONE'
          value: '+1 (555) 123-4567'
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
  }
}

// Alert: Email failure rate
resource emailFailureAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${baseName}-email-failure-alert-${environment}'
  location: 'global'
  properties: {
    description: 'Alert when email failure rate exceeds 5%'
    severity: 2
    enabled: true
    scopes: [
      functionApp.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'FailureRate'
          metricName: 'FunctionExecutionUnits'
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Average'
        }
      ]
    }
    actions: []
  }
}

// Outputs
output serviceBusNamespace string = serviceBusNamespace.name
output serviceBusConnectionString string = listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
output functionAppName string = functionApp.name
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output appInsightsConnectionString string = appInsights.properties.ConnectionString
