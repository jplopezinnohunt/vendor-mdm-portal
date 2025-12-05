# Hybrid Architecture Compliance - Invitation Feature

## 📋 Architecture Analysis

### ✅ Your Established Pattern

**Hybrid Database Architecture:**
```
┌────────────────────────────────────────────────────────────┐
│ REQUEST LIFECYCLE                                          │
├────────────────────────────────────────────────────────────┤
│ 1. SQL Database (Transactional)                            │
│    - Metadata: Status, IDs, References                     │
│    - State Management: Workflow states                     │
│    - Relationships: Foreign keys, indexes                  │
│                                                            │
│ 2. Cosmos DB (Artifacts & Events)                          │
│    - Full Payloads: Complete request data                  │
│    - Audit Trail: Immutable event history                  │
│    - Flexible Schema: JSON documents                       │
│                                                            │
│ 3. Service Bus (Integration)                               │
│    - Async Processing: Email, SAP integration              │
│    - Event Publishing: Domain events                       │
└────────────────────────────────────────────────────────────┘
```

### ❌ Current Invitation Implementation (Incomplete)

**What's Implemented:**
- ✅ SQL: `VendorInvitations` table (metadata + state)
- ✅ Service Bus: `invitation-created` event publishing
- ✅ Azure Function: Email processing
- ❌ **Cosmos: No invitation artifacts**
- ❌ **Cosmos: No domain events for lifecycle**

**Missing Pattern:**
```
Current: SQL → Service Bus → Email
Missing: SQL → Cosmos (Artifact) → Cosmos (Event) → Service Bus
```

---

## 🔧 Required Updates

### 1. **Cosmos DB Containers**

Add to your existing Cosmos database:

```
Database: VendorMdm
├── ChangeRequestData (existing)
├── DomainEvents (existing)
└── InvitationArtifacts (NEW)
    ├── Partition Key: /invitationId
    ├── Purpose: Store complete invitation payloads
    └── TTL: Optional (e.g., 1 year for old invitations)
```

### 2. **Update InvitationService Pattern**

**Current Flow:**
```csharp
public async Task<CreateInvitationResponse> CreateInvitationAsync(...)
{
    // 1. Create SQL record ✅
    var invitation = new VendorInvitation { ... };
    _context.VendorInvitations.Add(invitation);
    await _context.SaveChangesAsync();

    // 2. Publish to Service Bus ✅
    await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);

    return response;
}
```

**Should Be (Following Your Pattern):**
```csharp
public async Task<CreateInvitationResponse> CreateInvitationAsync(...)
{
    // A. Create SQL Record (Metadata)
    var invitation = new VendorInvitation { ... };
    _context.VendorInvitations.Add(invitation);
    await _context.SaveChangesAsync();

    // B. Store Artifact in Cosmos (Full Payload)
    await SaveInvitationArtifactAsync(invitation.Id.ToString(), new {
        VendorLegalName = request.VendorLegalName,
        PrimaryContactEmail = request.PrimaryContactEmail,
        InvitedBy = invitedBy,
        InvitedByName = invitedByName,
        Token = token,
        ExpiresAt = expiresAt,
        Notes = request.Notes,
        CreatedAt = DateTime.UtcNow,
        RequestMetadata = request // Full request object
    });

    // C. Emit Domain Event (Cosmos Events Container)
    await EmitEventAsync("InvitationCreated", invitation.Id.ToString(), new {
        InvitationId = invitation.Id,
        VendorName = request.VendorLegalName,
        Email = request.PrimaryContactEmail
    });

    // D. Publish to Service Bus (for email)
    await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);

    return response;
}
```

### 3. **Benefits of Following Pattern**

| Aspect | Without Cosmos | With Cosmos (Your Pattern) |
|--------|---------------|----------------------------|
| **Audit Trail** | Limited (SQL updates overwrite) | Complete immutable history |
| **Payload Storage** | Not stored | Full request data preserved |
| **Event Sourcing** | No event history | Complete event log |
| **Compliance** | Incomplete audit | Full audit compliance |
| **Debugging** | Limited historical data | Complete reconstruction possible |
| **Analytics** | SQL queries only | Rich JSON queries in Cosmos |

---

## 📝 Implementation Guide

### Step 1: Add Cosmos Injection to InvitationService

```csharp
public class InvitationService : IInvitationService
{
    private readonly SqlDbContext _context;
    private readonly ILogger<InvitationService> _logger;
    private readonly ServiceBusService _serviceBusService;
    private readonly Container _cosmosContainer;      // ADD
    private readonly Container _eventsContainer;       // ADD

    public InvitationService(
        SqlDbContext context, 
        ILogger<InvitationService> logger,
        ServiceBusService serviceBusService,
        CosmosClient cosmosClient)  // ADD
    {
        _context = context;
        _logger = logger;
        _serviceBusService = serviceBusService;
        _cosmosContainer = cosmosClient.GetContainer("VendorMdm", "InvitationArtifacts");  // ADD
        _eventsContainer = cosmosClient.GetContainer("VendorMdm", "DomainEvents");        // ADD
    }
}
```

### Step 2: Add Helper Methods (Following ArtifactService Pattern)

```csharp
private async Task SaveInvitationArtifactAsync(string invitationId, object payload)
{
    var artifact = new InvitationArtifact
    {
        Id = invitationId,
        InvitationId = invitationId,
        FullPayload = payload,
        CreatedAt = DateTime.UtcNow
    };
    await _cosmosContainer.UpsertItemAsync(artifact, new PartitionKey(invitationId));
}

private async Task EmitEventAsync(string eventType, string entityId, object data)
{
    var domainEvent = new DomainEvent
    {
        Id = Guid.NewGuid().ToString(),
        EventType = eventType,
        EntityId = entityId,
        Data = data,
        Timestamp = DateTime.UtcNow
    };

    // Store in Cosmos Events
    await _eventsContainer.CreateItemAsync(domainEvent, new PartitionKey(eventType));
    
    _logger.LogInformation("Domain event {EventType} emitted for {EntityId}", eventType, entityId);
}
```

### Step 3: Update CreateInvitationAsync

```csharp
public async Task<CreateInvitationResponse> CreateInvitationAsync(
    CreateInvitationRequest request, 
    Guid invitedBy, 
    string invitedByName)
{
    // ... existing validation code ...

    // Generate secure token
    var token = GenerateSecureToken();
    var expiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays);

    // A. SQL: Create invitation record (Metadata + State)
    var invitation = new VendorInvitation
    {
        Id = Guid.NewGuid(),
        InvitationToken = token,
        VendorLegalName = request.VendorLegalName,
        PrimaryContactEmail = request.PrimaryContactEmail,
        InvitedBy = invitedBy,
        InvitedByName = invitedByName,
        ExpiresAt = expiresAt,
        Status = InvitationStatus.Pending,
        Notes = request.Notes,
        CreatedAt = DateTime.UtcNow
    };

    _context.VendorInvitations.Add(invitation);
    await _context.SaveChangesAsync();

    // B. COSMOS: Store full payload (Artifact)
    await SaveInvitationArtifactAsync(invitation.Id.ToString(), new
    {
        VendorLegalName = request.VendorLegalName,
        PrimaryContactEmail = request.PrimaryContactEmail,
        InvitedBy = invitedBy,
        InvitedByName = invitedByName,
        Token = token,
        ExpiresAt = expiresAt,
        Notes = request.Notes,
        ExpirationDays = request.ExpirationDays,
        OriginalRequest = request
    });

    // C. COSMOS: Emit domain event
    await EmitEventAsync("InvitationCreated", invitation.Id.ToString(), new
    {
        InvitationId = invitation.Id,
        VendorName = request.VendorLegalName,
        Email = request.PrimaryContactEmail,
        InvitedBy = invitedByName
    });

    // D. SERVICE BUS: Queue email notification
    try
    {
        var emailMessage = new { ... };
        await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to queue email for invitation {InvitationId}", invitation.Id);
    }

    return new CreateInvitationResponse { ... };
}
```

### Step 4: Update CompleteInvitationAsync

```csharp
public async Task<bool> CompleteInvitationAsync(string token, Guid vendorApplicationId)
{
    var invitation = await _context.VendorInvitations
        .FirstOrDefaultAsync(i => i.InvitationToken == token);

    if (invitation == null || invitation.Status == InvitationStatus.Completed)
    {
        return false;
    }

    // A. SQL: Update state
    invitation.Status = InvitationStatus.Completed;
    invitation.CompletedAt = DateTime.UtcNow;
    invitation.VendorApplicationId = vendorApplicationId;
    await _context.SaveChangesAsync();

    // B. COSMOS: Store completion artifact
    await _cosmosContainer.UpsertItemAsync(new InvitationCompletionArtifact
    {
        Id = Guid.NewGuid().ToString(),
        InvitationId = invitation.Id.ToString(),
        VendorApplicationId = vendorApplicationId.ToString(),
        CompletedAt = DateTime.UtcNow
    }, new PartitionKey(invitation.Id.ToString()));

    // C. COSMOS: Emit completion event
    await EmitEventAsync("InvitationCompleted", invitation.Id.ToString(), new
    {
        InvitationId = invitation.Id,
        VendorApplicationId = vendorApplicationId,
        CompletedAt = DateTime.UtcNow
    });

    return true;
}
```

---

## 📊 Complete Flow Comparison

### Before (Incomplete):
```
Admin Creates Invitation
        ↓
    [SQL Only]
    VendorInvitations table
        ↓
    [Service Bus]
    invitation-emails queue
        ↓
    [Azure Function]
    Send Email
```

### After (Following Your Pattern):
```
Admin Creates Invitation
        ↓
    [SQL - State]
    VendorInvitations table (metadata/status)
        ↓
    [Cosmos - Artifact]
    InvitationArtifacts container (full payload)
        ↓
    [Cosmos - Event]
    DomainEvents container (InvitationCreated event)
        ↓
    [Service Bus]
    invitation-emails queue
        ↓
    [Azure Function]
    Send Email
```

---

## ✅ Architecture Compliance Checklist

- [ ] Cosmos DB: InvitationArtifacts container created
- [ ] InvitationService: CosmosClient injected
- [ ] CreateInvitationAsync: Artifact storage added
- [ ] CreateInvitationAsync: Domain event emission added
- [ ] CompleteInvitationAsync: Completion artifact added
- [ ] CompleteInvitationAsync: Domain event emission added
- [ ] ResendInvitationAsync: Domain event added
- [ ] All operations follow SQL → Cosmos → Service Bus pattern

---

## 🎯 Recommendation

**YES, you need to update the invitation implementation to follow your hybrid architecture pattern.**

**Why it matters:**
1. **Consistency**: All features should follow same pattern
2. **Audit Compliance**: Complete immutable event history
3. **Debugging**: Full payload reconstruction
4. **Querying**: Rich Cosmos DB queries for analytics
5. **Event Sourcing**: Proper domain event tracking

**Impact:**
- Minor code changes (add Cosmos calls)
- Better alignment with existing architecture
- Future-proof for compliance requirements

Would you like me to update the InvitationService to properly follow this pattern?
