@description('The environment name (dev, qa, prod)')
param environmentName string

@description('The Azure region')
param location string

var namespaceName = 'mdmportal-sb-${environmentName}'
var topicName = 'vendor-changes'
var subscriptionName = 'sap-integration-${environmentName}'
var invitationQueueName = 'invitation-emails'

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

// Queue for invitation emails
resource invitationQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: sbNamespace
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
  }
}

output serviceBusNamespaceName string = sbNamespace.name
output invitationQueueName string = invitationQueue.name
