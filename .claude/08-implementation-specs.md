# Implementation Specifications

---

## 8.1 Spec-Driven Development Protocol

### MANDATORY Process
All features MUST follow this sequence:

```
1. Create spec file: specs/{feature-name}.md
2. Spec includes: Requirements, Interfaces, Tests, Acceptance Criteria
3. Get spec approved (or self-review)
4. Implement according to spec
5. Verify against acceptance criteria
6. Commit with spec reference
```

### Spec Template
```markdown
# Feature: {Feature Name}

## Requirements
- REQ-001: {Requirement description}
- REQ-002: {Requirement description}

## Interfaces
```csharp
public interface I{FeatureName}Service
{
    Task<Result<T>> MethodAsync(params);
}
```

## Implementation Notes
- {Key implementation decisions}

## Test Cases
- TEST-001: {Test description} → Expected: {outcome}
- TEST-002: {Test description} → Expected: {outcome}

## Acceptance Criteria
- [ ] All tests pass
- [ ] Build succeeds (0 errors)
- [ ] Code review approved
```

---

## 8.2 Week-by-Week Specifications

### Week 2: Observability Core

**Packages Required**:
- OpenTelemetry.Extensions.Hosting
- OpenTelemetry.Instrumentation.AspNetCore
- OpenTelemetry.Instrumentation.Http
- OpenTelemetry.Instrumentation.SqlClient
- Azure.Monitor.OpenTelemetry.Exporter

**Interfaces**:
```csharp
public interface IDistributedTracing
{
    Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal);
    void SetTag(string key, object value);
    void RecordException(Exception ex);
}
```

**Metrics to Implement**:
| Metric | Type | Description |
|--------|------|-------------|
| request_count | Counter | Total HTTP requests |
| request_duration | Histogram | Request latency |
| active_requests | Gauge | Currently processing |
| error_count | Counter | Total errors |
| vendors_created | Counter | Business metric |
| invitations_sent | Counter | Business metric |

---

### Week 3: Migration to Core.Framework

**Tasks**:
1. Update VendorMdm.Api to reference Core.Framework
2. Replace custom auth with IAuthenticationService
3. Implement repository interfaces
4. Add Polly policies to external calls
5. Replace ILogger with IStructuredLogger

**Service Migration Pattern**:
```csharp
// BEFORE
public class VendorService
{
    private readonly ILogger<VendorService> _logger;

    public VendorService(ILogger<VendorService> logger)
    {
        _logger = logger;
    }
}

// AFTER
public class VendorService
{
    private readonly IStructuredLogger _logger;

    public VendorService(IStructuredLogger logger)
    {
        _logger = logger;
    }

    public async Task CreateVendorAsync(Vendor vendor)
    {
        _logger.LogInformation(
            "Creating vendor",
            ("VendorId", vendor.Id),
            ("LegalName", vendor.LegalName),
            ("VendorType", vendor.VendorType)
        );
    }
}
```

---

### Week 4: Health Checks + Document Registry

**Health Endpoints**:
| Endpoint | Purpose | Kubernetes Probe |
|----------|---------|------------------|
| /health/live | App is alive | Liveness |
| /health/ready | App can serve traffic | Readiness |
| /health/startup | App finished starting | Startup |

**Document Registry Interface**:
```csharp
public interface IDocumentRegistryService
{
    Task<Result<DocumentMetadata>> UploadAsync(
        Stream content,
        string fileName,
        string entityType,
        Guid entityId,
        Dictionary<string, string>? metadata = null);

    Task<Result<Stream>> DownloadAsync(Guid documentId);

    Task<Result<IEnumerable<DocumentMetadata>>> ListByEntityAsync(
        string entityType,
        Guid entityId);

    Task<Result> DeleteAsync(Guid documentId);

    Task<Result<OcrResult>> ExtractTextAsync(Guid documentId);

    Task<Result<ScanResult>> ScanForVirusAsync(Guid documentId);
}
```

---

### Week 5: API Versioning + Change Request

**API Versioning Configuration**:
```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// URL Pattern: /api/v1/vendors, /api/v2/vendors
```

**Change Request State Machine**:
```
Draft → Submitted → UnderReview → Approved → Integrated
                  ↘ Rejected
```

**Change Request Interface**:
```csharp
public interface IChangeRequestService
{
    Task<Result<ChangeRequest>> CreateAsync(CreateChangeRequestDto dto);
    Task<Result> SubmitAsync(Guid changeRequestId);
    Task<Result> ApproveAsync(Guid changeRequestId, string approvedBy, string? comments);
    Task<Result> RejectAsync(Guid changeRequestId, string rejectedBy, string reason);
    Task<Result> IntegrateToSapAsync(Guid changeRequestId);
    Task<Result<IEnumerable<ApprovalHistory>>> GetApprovalHistoryAsync(Guid changeRequestId);
}
```

---

### Week 6: Distributed Tracing + SAP Integration

**SAP RFC Operations**:
| RFC | Purpose |
|-----|---------|
| ZBAPI_VENDOR_CREATE | Create vendor in SAP |
| ZBAPI_VENDOR_UPDATE | Update vendor in SAP |
| ZBAPI_VENDOR_GETDETAIL | Get vendor from SAP |

**Bidirectional Sync Strategy**:
```
VendorMDM → SAP: On create/update, push to SAP via RFC
SAP → VendorMDM: Scheduled job pulls changes every 1 hour
Conflict Resolution: Last-write-wins with manual override option
```

**Circuit Breaker Config**:
```csharp
Policy
    .Handle<SapException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromMinutes(1)
    );
```

---

### Week 7: React Query + Multi-Stage Approval

**React Query Setup**:
```typescript
// QueryClient configuration
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: 10 * 60 * 1000, // 10 minutes
      retry: 3,
    },
  },
});

// Example hook
export function useVendors() {
  return useQuery({
    queryKey: ['vendors'],
    queryFn: () => api.get('/api/v1/vendors'),
  });
}
```

**Multi-Stage Approval Configuration**:
```csharp
public class ApprovalWorkflowConfig
{
    public int Stages { get; set; } // 1, 2, or 3
    public ApprovalMode Mode { get; set; } // Parallel or Sequential
    public List<ApprovalStage> StageDefinitions { get; set; }
}

public class ApprovalStage
{
    public int StageNumber { get; set; }
    public string RoleRequired { get; set; } // "Approver", "Manager", "Director"
    public int MinimumApprovers { get; set; }
}
```

---

### Week 8: Testing + Notifications

**Testing Stack**:
| Tool | Purpose |
|------|---------|
| Vitest | Unit tests (frontend) |
| React Testing Library | Component tests |
| Playwright | E2E tests |
| xUnit | Unit tests (backend) |
| Moq | Mocking (backend) |

**Notification Interface**:
```csharp
public interface INotificationService
{
    Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true);

    Task<Result> SendSmsAsync(
        string phoneNumber,
        string message);

    Task<Result> SendPushAsync(
        string deviceToken,
        string title,
        string body);
}
```

**Notification Templates**:
- InvitationSent
- ApprovalRequired
- VendorApproved
- VendorRejected
- DocumentUploaded

---

### Week 9: Background Jobs + Reporting

**Hangfire Jobs**:
| Job | Schedule | Description |
|-----|----------|-------------|
| SapSyncJob | Every 1 hour | Sync with SAP |
| EmailQueueJob | Every 1 minute | Process email queue |
| ReportGenerationJob | Daily at 2 AM | Generate daily reports |
| DataCleanupJob | Weekly | Clean old invitations |

**Reporting Interface**:
```csharp
public interface IReportingService
{
    Task<VendorStatistics> GetVendorStatisticsAsync(DateRange range);
    Task<InvitationStatistics> GetInvitationStatisticsAsync(DateRange range);
    Task<ApprovalStatistics> GetApprovalStatisticsAsync(DateRange range);
    Task<byte[]> ExportToExcelAsync(ReportType type, DateRange range);
    Task<byte[]> ExportToPdfAsync(ReportType type, DateRange range);
}
```

---

### Week 10: Feature Flags + Deployment

**Feature Flags**:
| Flag | Description |
|------|-------------|
| enable-sap-sync | Toggle SAP integration |
| enable-multi-stage-approval | Toggle new workflow |
| enable-notifications | Toggle notification system |
| enable-document-ocr | Toggle OCR feature |

**Blue-Green Deployment**:
```yaml
# Azure Deployment Slots
Production: vendorportal.azurewebsites.net
Staging: vendorportal-staging.azurewebsites.net

# Deployment Process
1. Deploy to Staging slot
2. Run smoke tests against Staging
3. If tests pass, swap slots
4. If issues, swap back (rollback)
```

---

## 8.3 Pattern Implementation Checklist

### Implemented (18 Patterns)
- [x] Hexagonal Architecture
- [x] Hybrid Relational-Document Model
- [x] Ontology-Driven Development
- [x] Result Pattern
- [x] Structured Logging
- [x] Multi-Channel Authentication
- [x] Role-Based Authorization
- [x] Audit Trail
- [x] Soft Delete
- [x] State Machines
- [x] File Storage Abstraction
- [x] SAP RFC Integration
- [x] Email Templating
- [x] Event Sourcing (partial)
- [x] Data Residency
- [x] Multi-Tenancy
- [x] PII Masking
- [x] GDPR Compliance

### Missing (8 Patterns)
- [ ] API Versioning (Week 5)
- [ ] Rate Limiting (Week 2)
- [ ] Circuit Breaker (Week 3)
- [ ] Response Caching (Week 6)
- [ ] Background Jobs (Week 9)
- [ ] Feature Flags (Week 10)
- [ ] Code Splitting (Week 7)
- [ ] Distributed Tracing (Week 6)

---

## 8.4 Functional Items Checklist

| Item | Week | Status |
|------|------|--------|
| Document Registry Service | 4 | Pending |
| Change Request Workflow | 5 | Pending |
| SAP Vendor Sync | 6 | Pending |
| Multi-Stage Approval | 7 | Pending |
| Notification System | 8 | Pending |
| Reporting & Analytics | 9 | Pending |

---

## 8.5 Migration Size Limits

**CRITICAL**: All EF Core migrations MUST be < 50KB

```bash
# Check migration size before commit
ls -lh backend/VendorMdm.Api/Migrations/*.cs | grep -v Designer | grep -v Snapshot
```

If migration exceeds 50KB:
1. Split into multiple smaller migrations
2. Review for unnecessary changes
3. Consider manual SQL for large data operations

---

## 8.6 Test Requirements

### Backend Test Coverage
- Unit tests: 80% minimum
- Integration tests: Critical paths
- Each service method must have tests

### Frontend Test Coverage
- Component tests: All forms and lists
- E2E tests: Critical user journeys
- Accessibility tests: WCAG 2.1 AA

### Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public async Task CreateVendorAsync_ValidInput_ReturnsSuccess()
{
    // Arrange, Act, Assert
}

[Fact]
public async Task CreateVendorAsync_DuplicateTaxId_ReturnsFailure()
{
    // Arrange, Act, Assert
}
```
