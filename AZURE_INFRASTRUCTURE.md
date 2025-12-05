# Azure Components for Invitation-Based Onboarding

## Overview
This document describes all Azure components required to support the vendor invitation-based onboarding feature.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Frontend (Azure Static Web Apps)             │
│  - Invitation Management UI                                          │
│  - Vendor Registration Form                                          │
└───────────────────┬─────────────────────────────────────────────────┘
                    │ HTTPS/REST API
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                  API Layer (Azure Container Apps / App Service)      │
│  - InvitationController                                              │
│  - InvitationService                                                 │
│  - ServiceBusService                                                 │
└───────┬──────────────────────────┬──────────────────────────────────┘
        │                          │
        │                          │ Publish Message
        ▼                          ▼
┌──────────────────┐    ┌─────────────────────────────────┐
│  Azure SQL DB    │    │  Azure Service Bus              │
│                  │    │  Queue: invitation-emails       │
│  Tables:         │    │                                 │
│  - VendorInv...  │    │  Message Format:                │
│  - VendorApp...  │    │  {                              │
│                  │    │    invitationId,                │
│                  │    │    vendorName,                  │
│                  │    │    email,                       │
│                  │    │    token,                       │
│                  │    │    expiresAt                    │
│                  │    │  }                              │
└──────────────────┘    └────────────┬────────────────────┘
                                     │ Service Bus Trigger
                                     ▼
                        ┌────────────────────────────────────┐
                        │  Azure Functions                   │
                        │  (VendorMdm.Artifacts)            │
                        │                                    │
                        │  Function: SendInvitationEmail    │
                        │  - Triggered by Service Bus       │
                        │  - Builds HTML email              │
                        │  - Sends via Communication Svc    │
                        └─────────────┬──────────────────────┘
                                      │
                                      ▼
                        ┌────────────────────────────────────┐
                        │  Azure Communication Services      │
                        │  or SendGrid                       │
                        │                                    │
                        │  - Sends HTML email to vendor      │
                        │  - Tracks delivery status          │
                        └────────────────────────────────────┘
```

## Required Azure Resources

### 1. Azure SQL Database
**Resource Name:** `vendormdm-sql-db`  
**Purpose:** Store invitation and application data

#### New Table: VendorInvitations
```sql
CREATE TABLE VendorInvitations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InvitationToken NVARCHAR(100) NOT NULL UNIQUE,
    VendorLegalName NVARCHAR(200) NOT NULL,
    PrimaryContactEmail NVARCHAR(255) NOT NULL,
    InvitedBy UNIQUEIDENTIFIER NOT NULL,
    InvitedByName NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CompletedAt DATETIME2 NULL,
    VendorApplicationId UNIQUEIDENTIFIER NULL,
    Notes NVARCHAR(1000) NULL,
    
    CONSTRAINT FK_VendorInvitations_VendorApplications 
        FOREIGN KEY (VendorApplicationId) 
        REFERENCES VendorApplications(Id)
);

CREATE INDEX IX_VendorInvitations_Token ON VendorInvitations(InvitationToken);
CREATE INDEX IX_VendorInvitations_Email ON VendorInvitations(PrimaryContactEmail);
CREATE INDEX IX_VendorInvitations_Status ON VendorInvitations(Status);
CREATE INDEX IX_VendorInvitations_ExpiresAt ON VendorInvitations(ExpiresAt);
```

#### Updated Table: VendorApplications
```sql
ALTER TABLE VendorApplications
ADD TaxId NVARCHAR(100) NULL,
    ContactName NVARCHAR(200) NULL,
    RegistrationType NVARCHAR(20) NOT NULL DEFAULT 'SelfRegistration',
    InvitationId UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2 NULL;

ALTER TABLE VendorApplications
ADD CONSTRAINT FK_VendorApplications_Invitations
    FOREIGN KEY (InvitationId)
    REFERENCES VendorInvitations(Id);
```

**Configuration:**
- SKU: Standard S1 or higher
- Backup: Enabled (Point-in-time restore)
- Connection String: Store in Azure Key Vault

### 2. Azure Service Bus
**Resource Name:** `vendormdm-servicebus-ns`  
**Purpose:** Message queue for asynchronous email sending

#### New Queue: invitation-emails
**Configuration:**
- Max Queue Size: 1 GB
- Message TTL: 14 days (default)
- Lock Duration: 5 minutes
- Max Delivery Count: 10
- Dead Letter Queue: Enabled
- Duplicate Detection: 10 minutes window

**Message Schema:**
```json
{
  "EventType": "invitation-created",
  "Data": {
    "InvitationId": "guid",
    "VendorName": "string",
    "Email": "string",
    "Token": "string",
    "ExpiresAt": "ISO-8601 datetime",
    "InvitedByName": "string",
    "CompanyName": "string",
    "Notes": "string?"
  }
}
```

**Application Properties:**
- `sapEnvironmentCode`: D01, Q01, P01
- `eventType`: invitation-created

#### Existing Queue: vendor-changes
Remains unchanged for vendor data modification requests.

**Pricing Tier:** Standard (required for Topics/Subscriptions if needed)

### 3. Azure Functions (VendorMdm.Artifacts)
**Resource Name:** `vendormdm-functions-app`  
**Purpose:** Serverless email processing

#### Function: SendInvitationEmail
- **Trigger:** Service Bus Queue (`invitation-emails`)
- **Runtime:** .NET 8 (Isolated)
- **Timeout:** 5 minutes
- **Retry Policy:** Exponential backoff (Service Bus handles)
- **Inputs:** Service Bus message
- **Outputs:** Email via Azure Communication Services

**App Settings Required:**
```json
{
  "ServiceBusConnection": "<connection-string>",
  "EmailServiceConnection": "<azure-communication-services-connection>",
  "APP_BASE_URL": "https://vendor-portal.yourcompany.com",
  "COMPANY_NAME": "Your Company Name",
  "SUPPORT_EMAIL": "vendorsupport@yourcompany.com",
  "SUPPORT_PHONE": "+1 (555) 123-4567"
}
```

**Function Details:**
```csharp
[Function("SendInvitationEmail")]
public async Task SendInvitationEmailFromQueue(
    [ServiceBusTrigger("invitation-emails", Connection = "ServiceBusConnection")] 
    string message)
{
    // Parse message
    // Build HTML email
    // Send via Azure Communication Services
    // Log success/failure
}
```

### 4. Azure Communication Services (Email)
**Resource Name:** `vendormdm-communication-svc`  
**Purpose:** Enterprise email delivery

**Features:**
- Send transactional emails
- Custom domain support
- Delivery tracking
- Bounce/complaint handling

**Configuration:**
- Sender Domain: `noreply@yourcompany.com`
- Daily Send Limit: 10,000 emails (Standard tier)
- Verified Domain: Required for production

**Alternative:** SendGrid (if already in use)
- API Key stored in Key Vault
- Integration via NuGet package

### 5. Azure Key Vault
**Resource Name:** `vendormdm-keyvault`  
**Purpose:** Secure secrets management

**Secrets to Store:**
- `SqlConnectionString`: SQL Database connection
- `ServiceBusConnection`: Service Bus namespace connection
- `EmailServiceApiKey`: Communication Services or SendGrid key
- `JwtSigningKey`: For API authentication (future)

**Access Policy:**
- API App Service: Get, List secrets
- Azure Functions: Get, List secrets
- Developers: List only (not Get values in production)

### 6. Application Insights
**Resource Name:** `vendormdm-appinsights`  
**Purpose:** Monitoring and diagnostics

**Key Metrics to Track:**
- Invitation creation rate
- Email delivery success rate
- Token validation failures
- API response times
- Error rates by component

**Custom Events:**
```csharp
telemetry.TrackEvent("InvitationCreated", 
    new Dictionary<string, string> {
        { "InvitationId", id },
        { "VendorEmail", email },
        { "InvitedBy", userName }
    });

telemetry.TrackEvent("InvitationEmailSent",
    new Dictionary<string, string> {
        { "InvitationId", id },
        { "Status", "Success" }
    });
```

**Alerts:**
1. Email failure rate > 5%
2. Invitation creation errors
3. Service Bus queue depth > 1000
4. Function execution failures

### 7. Azure Static Web Apps (Frontend)
**Resource Name:** `vendormdm-webapp`  
**Purpose:** Host React application

**Features Used:**
- Built-in CDN
- Custom domains
- API integration (routes to Function App or API)
- Environment variables

**Configuration:**
- Build Preset: React
- App Location: `/frontend`
- Output Location: `dist`
- API Location: N/A (separate API)

**Environment Variables:**
```
VITE_API_BASE_URL=/api
```

### 8. Azure Container Apps / App Service (API)
**Resource Name:** `vendormdm-api-app`  
**Purpose:** Host .NET API (VendorMdm.Api)

**Configuration:**
- Runtime: .NET 8
- Always On: Enabled
- HTTPS Only: Enabled
- Managed Identity: System-assigned (for Key Vault access)

**App Settings:**
```json
{
  "ConnectionStrings__Sql": "@Microsoft.KeyVault(SecretUri=...)",
  "ConnectionStrings__ServiceBus": "@Microsoft.KeyVault(SecretUri=...)",
  "SapEnvironmentCode": "D01",
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "..."
}
```

## Deployment Architecture

### Development Environment
- API: Local (Kestrel)
- Functions: Azure Functions Core Tools
- Database: Local SQL Server or Azure SQL (Dev tier)
- Service Bus: Shared namespace (Dev queue)
- Emails: Console logging (no actual sending)

### Staging/QA Environment
- API: Azure App Service (B1 tier)
- Functions: Consumption plan
- Database: Azure SQL (S1 tier)
- Service Bus: Standard tier
- Emails: Test domain (Azure Communication Services)

### Production Environment
- API: Azure Container Apps or App Service (P1V3 tier)
- Functions: Premium plan (for VNET integration)
- Database: Azure SQL (S3+ tier with read replicas)
- Service Bus: Premium tier (for better throughput)
- Emails: Production domain with monitoring

## Infrastructure as Code (Bicep/Terraform)

### Example: Creating Service Bus Queue
```bicep
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: 'vendormdm-servicebus-ns'
}

resource invitationEmailQueue 'Microsoft.ServiceBus/namespaces/queues@2021-11-01' = {
  parent: serviceBusNamespace
  name: 'invitation-emails'
  properties: {
    maxSizeInMegabytes: 1024
    defaultMessageTimeToLive: 'P14D'
    lockDuration: 'PT5M'
    maxDeliveryCount: 10
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    deadLetteringOnMessageExpiration: true
  }
}
```

## Cost Estimation

| Resource | Tier | Est. Monthly Cost (USD) |
|----------|------|-------------------------|
| Azure SQL Database (S1) | Standard | $15 |
| Service Bus Namespace | Standard | $10 + $0.05/million ops |
| Azure Functions | Consumption | $0-20 (based on executions) |
| Communication Services | Standard | $0.0002/email |
| Key Vault | Standard | $0.03/10k operations |
| Application Insights | Basic | $2-5/GB ingested |
| Static Web Apps | Free/Standard | $0-9 |
| App Service (B1) | Basic | $13 |
| **Total (Development)** | | ~$45-75/month |
| **Total (Production)** | | ~$150-300/month |

## Security Considerations

1. **Network Security:**
   - API: Behind Azure Front Door or App Gateway
   - Functions: VNET integration for private access
   - SQL: Private endpoint, no public access

2. **Authentication:**
   - API: Azure AD B2C or MSAL
   - Functions: System-assigned managed identity
   - Service Bus: Connection string in Key Vault

3. **Data Protection:**
   - Encryption at rest: Enabled on SQL, Service Bus
   - Encryption in transit: TLS 1.2 minimum
   - PII in emails: Minimal exposure

4. **Access Control:**
   - RBAC: Least privilege principle
   - Key Vault: Role-based access
   - SQL: Contained database users

## Monitoring & Alerts

### Key Metrics Dashboard
1. Invitation funnel:
   - Created → Sent → Validated → Completed
2. Email delivery:
   - Queued → Sent → Delivered → Bounced
3. System health:
   - API availability
   - Function success rate
   - Queue depth

### Alert Rules
```kusto
// Email failures > 5% in 15 minutes
let totalEmails = requests
| where name == "SendInvitationEmail"
| count;

let failedEmails = requests
| where name == "SendInvitationEmail" and success == false
| count;

(failedEmails / totalEmails) > 0.05
```

## Disaster Recovery

### Backup Strategy
- **Database**: Automated backups (7-day retention)
- **Configuration**: Infrastructure as Code (Git repository)
- **Secrets**: Key Vault soft-delete enabled

### Recovery Procedures
1. **Database failure**: Restore from point-in-time backup
2. **Service Bus**: Messages in dead-letter queue preserved
3. **Function App**: Redeploy from CI/CD pipeline

### RTO/RPO Targets
- **RTO** (Recovery Time Objective): 4 hours
- **RPO** (Recovery Point Objective): 1 hour

## Scaling Considerations

### Auto-Scaling Rules
- **API**: Scale out when CPU > 70%
- **Functions**: Automatic (consumption plan)
- **Service Bus**: Premium for higher throughput

### Performance Targets
- API response time: < 200ms (p95)
- Email delivery: < 5 minutes (p95)
- Database queries: < 50ms (p95)

## Next Steps

1. ✅ Create Azure resources (use Bicep templates)
2. ✅ Configure Service Bus queue
3. ✅ Deploy Azure Function
4. ⏳ Set up Azure Communication Services or SendGrid
5. ⏳ Configure monitoring and alerts
6. ⏳ Test end-to-end flow
7. ⏳ Production deployment

## Related Documentation
- [Implementation Summary](./INVITATION_IMPLEMENTATION_SUMMARY.md)
- [Quick Start Guide](./INVITATION_QUICK_START.md)
- [API Documentation](./API_DOCUMENTATION.md)
