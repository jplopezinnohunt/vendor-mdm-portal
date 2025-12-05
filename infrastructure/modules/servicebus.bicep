@description('The environment name (dev, qa, prod)')
param environmentName string

@description('The Azure region')
param location string

var namespaceName = 'mdmportal-sb-${environmentName}'
var topicName = 'vendor-changes'
var subscriptionName = 'sap-integration-${environmentName}'

resource sbNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource sbTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: sbNamespace
  name: topicName
}

resource sbSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: sbTopic
  name: subscriptionName
}

output serviceBusNamespaceName string = sbNamespace.name
