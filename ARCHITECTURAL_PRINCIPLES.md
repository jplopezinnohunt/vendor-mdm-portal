# 🏛️ ARCHITECTURAL PRINCIPLES - MANDATORY PATTERNS

## ⚡ CORE PRINCIPLE: Hybrid Database Architecture

**THIS IS NOT OPTIONAL - THIS IS THE FOUNDATION OF OUR SYSTEM**

All features MUST follow the Hybrid Database Architecture pattern for:
- ✅ Event-Driven Architecture (EDA)
- ✅ Complete Audit Trail & Logging
- ✅ Schema Flexibility
- ✅ Metadata Separation

---

## 📐 THE PATTERN (MANDATORY)

### **SQL Database** (Transactional State)
**Purpose:** Metadata, State Management, Relationships

**Use for:**
- Entity status (Pending, Approved, Completed)
- Foreign key relationships
- Indexed queries for fast lookups
- Transactional consistency

**Examples:**
```csharp
// VendorApplication (SQL)
- Id, Status, CreatedAt (metadata)
- CompanyName, ContactEmail (searchable fields)
- InvitationId (FK relationship)

// VendorInvitation (SQL)
- Id, Status, ExpiresAt (metadata)
- Token (indexed for validation)  
- VendorApplicationId (FK relationship)
```

### **Cosmos DB - Artifacts Container** (Payload Storage)
**Purpose:** Complete request payloads, flexible schema

**Use for:**
- Full JSON payloads
- Schema evolution without migrations
- Complete data reconstruction
- Compliance & audit requirements

**Examples:**
```csharp
// InvitationArtifacts (Cosmos)
{
  "id": "invitation-guid",
  "invitationId": "invitation-guid", // Partition key
  "fullPayload": {
    "vendorLegalName": "...",
    "primaryContactEmail": "...",
    "originalRequest": { /* complete request */ }
  },
  "createdAt": "2025-12-05T..."
}
```

### **Cosmos DB - Events Container** (Event Sourcing)
** Purpose:** Domain events for event-driven architecture

**Use for:**
- Event sourcing
- Audit trail
- System integration
- Analytics & reporting

**Examples:**
```csharp
// DomainEvents (Cosmos)
{
  "id": "event-guid",
  "eventType": "InvitationCreated", // Partition key
  "entityId": "invitation-guid",
  "timestamp": "2025-12-05T...",
  "data": {
    "invitationId": "...",
    "vendorName": "...",
    "email": "..."
  }
}
```

### **Service Bus** (Async Integration)
**Purpose:** Asynchronous processing, system integration

**Use for:**
- Email notifications
- SAP integration triggers
- Cross-system events
- Retry logic & dead-letter handling

---

## 🔄 MANDATORY FLOW FOR ALL FEATURES

```
┌─────────────────────────────────────────────────────────┐
│ USER ACTION (Create, Update, Complete, etc.)            │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│ A. SQL DATABASE (State & Metadata)                       │
│    - Create/Update entity                               │
│    - Set status, timestamps                             │
│    - Save relationships                                 │
│    - Commit transaction                                 │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│ B. COSMOS DB - Artifacts (Full Payload)                  │
│    - Store complete request object                      │
│    - Include all metadata                               │
│    - Enable future reconstruction                       │
│    - Non-blocking (catch exceptions)                    │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│ C. COSMOS DB - Events (Domain Event)                     │
│    - Emit domain event                                  │
│    - Event type as partition key                        │
│    - Include relevant data                              │
│    - Non-blocking (catch exceptions)                    │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────┐
│ D. SERVICE BUS (Optional - Integration)                  │
│    - Publish for async processing                       │
│    - Email, SAP, notifications                          │
│    - Non-blocking (catch exceptions)                    │
└─────────────────────────────────────────────────────────┘
```

---

## 📝 CODE TEMPLATE (MANDATORY)

### **Service Constructor Pattern**

```csharp
public class YourService : IYourService
{
    private readonly SqlDbContext _context;
    private readonly ILogger<YourService> _logger;
    private readonly Container _cosmosArtifactsContainer;
    private readonly Container _cosmosEventsContainer;
    private readonly ServiceBusService _serviceBusService; // Optional

    public YourService(
        SqlDbContext context,
        ILogger<YourService> logger,
        CosmosClient cosmosClient,
        ServiceBusService serviceBusService = null) // Optional
    {
        _context = context;
        _logger = logger;
        _cosmosArtifactsContainer = cosmosClient.GetContainer("VendorMdm", "YourArtifacts");
        _cosmosEventsContainer = cosmosClient.GetContainer("VendorMdm", "DomainEvents");
        _serviceBusService = serviceBusService;
    }
}
```

### **Create Operation Pattern**

```csharp
public async Task<YourResponse> CreateAsync(YourRequest request)
{
    // STEP A: SQL - Create entity (Metadata & State)
    var entity = new YourEntity
    {
        Id = Guid.NewGuid(),
        Status = "Pending",
        CreatedAt = DateTime.UtcNow,
        // ... metadata fields
    };

    _context.YourEntities.Add(entity);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Entity created: {EntityId}", entity.Id);

    // STEP B: COSMOS - Store artifact (Full Payload) - NON-BLOCKING
    try
    {
        await SaveArtifactAsync(entity.Id.ToString(), new
        {
            EntityId = entity.Id,
            FullPayload = request,
            Metadata = new { /* all relevant data */ },
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Artifact stored for {EntityId}", entity.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to store artifact for {EntityId}", entity.Id);
        // CONTINUE - don't block on artifact failure
    }

    // STEP C: COSMOS - Emit event (Event Sourcing) - NON-BLOCKING
    try
    {
        await EmitDomainEventAsync("EntityCreated", entity.Id.ToString(), new
        {
            EntityId = entity.Id,
            // ... event data
        });

        _logger.LogInformation("Domain event emitted for {EntityId}", entity.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to emit event for {EntityId}", entity.Id);
        // CONTINUE - don't block on event failure
    }

    // STEP D: SERVICE BUS (Optional) - NON-BLOCKING
    if (_serviceBusService != null)
    {
        try
        {
            await _serviceBusService.PublishEventAsync("your-event-type", /* message */);
            _logger.LogInformation("Message published for {EntityId}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message for {EntityId}", entity.Id);
            // CONTINUE - don't block on message failure
        }
    }

    return new YourResponse { /* ... */ };
}
```

### **Helper Methods Pattern** (Copy from ArtifactService)

```csharp
/// <summary>
/// Store artifact in Cosmos DB for complete audit trail
/// </summary>
private async Task SaveArtifactAsync(string entityId, object payload)
{
    var artifact = new YourArtifact
    {
        Id = entityId,
        EntityId = entityId, // Partition key
        FullPayload = payload,
        CreatedAt = DateTime.UtcNow
    };

    await _cosmosArtifactsContainer.UpsertItemAsync(
        artifact,
        new PartitionKey(entityId));
}

/// <summary>
/// Emit domain event to Cosmos DB for event sourcing
/// </summary>
private async Task EmitDomainEventAsync(string eventType, string entityId, object data)
{
    var domainEvent = new DomainEvent
    {
        Id = Guid.NewGuid().ToString(),
        EventType = eventType, // Partition key
        EntityId = entityId,
        Timestamp = DateTime.UtcNow,
        Data = data
    };

    await _cosmosEventsContainer.CreateItemAsync(
        domainEvent,
        new PartitionKey(eventType));
}
```

---

## 🚫 COMMON MISTAKES TO AVOID

### ❌ **Wrong:** SQL Only
```csharp
// DON'T DO THIS!
var invitation = new VendorInvitation { /* ... */ };
_context.VendorInvitations.Add(invitation);
await _context.SaveChangesAsync();
return response; // Missing Cosmos artifacts & events!
```

### ✅ **Correct:** Hybrid Pattern
```csharp
// DO THIS!
// A. SQL
var invitation = new VendorInvitation { /* ... */ };
_context.VendorInvitations.Add(invitation);
await _context.SaveChangesAsync();

// B. Cosmos Artifact
await SaveArtifactAsync(invitation.Id.ToString(), fullPayload);

// C. Cosmos Event
await EmitDomainEventAsync("InvitationCreated", invitation.Id.ToString(), eventData);

// D. Service Bus (optional)
await _serviceBusService.PublishEventAsync("invitation-created", message);
```

---

## 📊 WHY THIS IS MANDATORY

| Requirement | SQL Only | Hybrid Pattern |
|-------------|----------|----------------|
| **Event Sourcing** | ❌ No event history | ✅ Complete event log |
| **Audit Trail** | ⚠️ Limited (updates overwrite) | ✅ Immutable history |
| **Flexibility** | ❌ Schema migrations required | ✅ JSON schema evolution |
| **Compliance** | ⚠️ Incomplete audit | ✅ Full regulatory compliance |
| **Debugging** | ⚠️ Current state only | ✅ Complete reconstruction |
| **Analytics** | ⚠️ SQL queries only | ✅ Rich Cosmos queries |
| **Integration** | ❌ Tight coupling | ✅ Event-driven decoupling |

---

## 🏗️ INFRASTRUCTURE REQUIREMENTS

### Cosmos DB Containers (Required)
```
Database: VendorMdm
├── InvitationArtifacts       (for invitation feature)
├── VendorChangeArtifacts     (for vendor modifications)  
├── DomainEvents               (shared - all events)
└── ChangeRequestData          (existing)
```

### Partition Keys
- **Artifacts:** Entity ID (e.g., `invitationId`)
- **Events:** Event Type (e.g., `"InvitationCreated"`)

---

## ✅ CHECKLIST FOR NEW FEATURES

Before implementing ANY new feature:

- [ ] **SQL**: Entity design for metadata & state
- [ ] **Cosmos Artifacts**: Define artifact schema
- [ ] **Cosmos Events**: Identify domain events
- [ ] **Service**: Inject CosmosClient
- [ ] **Methods**: Follow A→B→C→D pattern
- [ ] **Logging**: Log each step
- [ ] **Error Handling**: Non-blocking for Cosmos/Service Bus
- [ ] **Testing**: Verify all 4 layers

---

## 🎯 REAL EXAMPLE: Invitation Feature

### Before (Incorrect - SQL Only) ❌
```csharp
public async Task<CreateInvitationResponse> CreateInvitationAsync(...)
{
    var invitation = new VendorInvitation { /* ... */ };
    _context.VendorInvitations.Add(invitation);
    await _context.SaveChangesAsync();
    
    await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);
    
    return response;
}
```

**Problems:**
- ❌ No Cosmos artifact storage
- ❌ No domain events
- ❌ No complete audit trail
- ❌ SQL updates will overwrite data

### After (Correct - Hybrid Pattern) ✅
```csharp
public async Task<CreateInvitationResponse> CreateInvitationAsync(...)
{
    // A. SQL: State & metadata
    var invitation = new VendorInvitation { /* ... */ };
    _context.VendorInvitations.Add(invitation);
    await _context.SaveChangesAsync();

    // B. COSMOS: Artifact (full payload)
    await SaveInvitationArtifactAsync(invitation.Id.ToString(), fullPayload);

    // C. COSMOS: Event (event sourcing)
    await EmitDomainEventAsync("InvitationCreated", invitation.Id.ToString(), eventData);

    // D. SERVICE BUS: Email notification
    await _serviceBusService.PublishEventAsync("invitation-created", emailMessage);

    return response;
}
```

**Benefits:**
- ✅ Complete audit trail
- ✅ Event sourcing enabled
- ✅ Schema flexibility
- ✅ Regulatory compliance

---

## 📚 REFERENCE IMPLEMENTATION

**See:** `backend/VendorMdm.Artifacts/Services/ArtifactService.cs`

This is the GOLD STANDARD implementation showing:
- Proper constructor injection
- `SaveCosmosPayloadAsync` helper
- `EmitEventAsync` helper
- Error handling
- Logging

**ALL new services MUST follow this pattern!**

---

## 🔒 ENFORCEMENT

**As of:** December 5, 2025  
**Status:** MANDATORY for all features  
**Review:** All PRs must demonstrate hybrid pattern compliance

**Non-compliance = No merge**

This is not a suggestion - this is the architectural foundation of the system.

---

## 📞 Questions?

If you're unsure how to implement this pattern:
1. Review `ArtifactService.cs`
2. Review `InvitationService.cs` (updated implementation)
3. Ask the architecture team

**Remember:** SQL→Cosmos Artifact→Cosmos Event→Service Bus

This is THE way. 🚀
