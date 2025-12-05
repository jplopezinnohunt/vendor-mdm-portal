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
    adminLogin: 'mdmadmin' // In real scenario, use KeyVault
    adminPassword: 'ChangeMe123!' // In real scenario, use KeyVault
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

// Role Assignments
// Assign Function App Managed Identity access to Cosmos
var cosmosAccountName = cosmos.outputs.cosmosAccountName
var roleAssignmentName = guid(cosmosAccountName, functionApp.outputs.functionAppPrincipalId, '00000000-0000-0000-0000-000000000002')

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' existing = {
  name: cosmosAccountName
}

resource cosmosRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = {
  name: '${cosmosAccount.name}/${roleAssignmentName}'
  properties: {
    roleDefinitionId: resourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', cosmosAccountName, '00000000-0000-0000-0000-000000000002')
    principalId: functionApp.outputs.functionAppPrincipalId
    scope: cosmosAccount.id
  }
}

output functionAppName string = functionApp.outputs.functionAppName
