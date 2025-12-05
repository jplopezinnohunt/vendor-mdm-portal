@description('The environment name (dev, qa, prod)')
param environmentName string = 'dev'

@description('The Azure region')
param location string = resourceGroup().location

module cosmos 'modules/cosmos.bicep' = {
  name: 'cosmosDeploy'
  params: {
    environmentName: environmentName
    location: location
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sqlDeploy'
  params: {
    environmentName: environmentName
    location: location
    sqlAdminLogin: 'mdmadmin' // In real scenario, use KeyVault
    sqlAdminPassword: 'ChangeMe123!' // In real scenario, use KeyVault
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'serviceBusDeploy'
  params: {
    environmentName: environmentName
    location: location
  }
}

module functionApp 'modules/functionapp.bicep' = {
  name: 'functionAppDeploy'
  params: {
    environmentName: environmentName
    location: location
  }
}

// Role Assignments (Simplified for example)
// Assign Function App Managed Identity access to Cosmos, SQL, Service Bus
// Note: SQL requires AAD Auth setup which is complex in Bicep alone, skipping for brevity
// Cosmos Role Assignment
// Cosmos Role Assignment
resource cosmosRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = {
  name: guid(cosmos.outputs.cosmosAccountName, functionApp.outputs.functionAppPrincipalId, '00000000-0000-0000-0000-000000000002') // Data Contributor Role
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', cosmos.outputs.cosmosAccountName, '00000000-0000-0000-0000-000000000002')
    principalId: functionApp.outputs.functionAppPrincipalId
    scope: resourceId('Microsoft.DocumentDB/databaseAccounts', cosmos.outputs.cosmosAccountName)
  }
}

output functionAppName string = functionApp.outputs.functionAppName
