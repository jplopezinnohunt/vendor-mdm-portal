# Complete Simulation Services - Implementation Plan

**Branch:** `feature/sap-api-simulation`  
**Strategy:** Interface-Based Simulation → Real Implementation Swap  
**Date:** December 20, 2025

---

## Architecture Overview

All external dependencies will be abstracted behind interfaces, allowing us to develop with **simulation services** and later swap to **real implementations** without changing business logic.

```
┌─────────────────────────────────────────────────────────────┐
│                    API Controllers                           │
│  (VendorController, WorkflowController, etc.)               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  Service Interfaces                          │
│  (ISapService, IAuthService, IMasterDataService, etc.)      │
└─────┬──────────────────────────────────────────────┬────────┘
      │                                                │
      ▼                                                ▼
┌──────────────────┐                        ┌──────────────────┐
│   SIMULATION     │                        │   REAL IMPL      │
│  (Phase 1)       │                        │   (Phase 2)      │
├──────────────────┤                        ├──────────────────┤
│ In-Memory Data   │                        │ SAP NCo/RFC      │
│ Mock Responses   │                        │ Azure AD         │
│ File System      │                        │ Azure Blob       │
│ Console Logging  │                        │ SendGrid         │
└──────────────────┘                        │ Service Bus      │
                                             └──────────────────┘
```

---

## 1. SAP Integration Services

### 1.1 Interface Definition

```csharp
public interface ISapVendorService
{
    // Search & Duplicate Detection
    Task<VendorSearchResponse> SearchVendorsAsync(VendorSearchRequest request);
    
    // Vendor CRUD
    Task<VendorGetResponse> GetVendorAsync(string vendorNumber, string companyCode);
    Task<VendorCreateResponse> CreateVendorAsync(VendorCreateRequest request);
    Task<VendorUpdateResponse> UpdateVendorAsync(string vendorNumber, VendorUpdateRequest request);
    
    // Validation
    Task<NameValidationResult> ValidateNameAsync(NameValidationRequest request);
    Task<BankValidationResult> ValidateBankAsync(BankValidationRequest request);
    
    // Bank Duplicate Check
    Task<BankDuplicateCheckResult> CheckBankDuplicateAsync(string iban, string companyCode);
}
```

### 1.2 Simulation Implementation

```csharp
public class SapVendorSimulationService : ISapVendorService
{
    private readonly ILogger<SapVendorSimulationService> _logger;
    private readonly ISapDataStore _dataStore;
    private readonly ILevenshteinMatcher _fuzzyMatcher;

    // In-memory mock data with ~100 vendors
    // Levenshtein-based fuzzy search
    // IBAN/SWIFT validation algorithms
    // Realistic latency simulation (100-500ms)
}
```

### 1.3 Real Implementation (Future)

```csharp
public class SapVendorRfcService : ISapVendorService
{
    private readonly ISapConnectionPool _sapConnection;
    
    // SAP NCo implementation
    // BAPI_VENDOR_CREATE1
    // BAPI_VENDOR_CHANGE
    // BAPI_VENDOR_GETDETAIL
}
```

---

## 2. RBAC / Authorization Services

### 2.1 Interface Definition

```csharp
public interface IAuthorizationService
{
    // User Role Management
    Task<UserRoles> GetUserRolesAsync(string userId);
    Task<bool> HasRoleAsync(string userId, string role);
    Task<bool> HasAnyRoleAsync(string userId, params string[] roles);
    
    // Permission Checks
    Task<bool> CanEditVendorAsync(string userId, string vendorId);
    Task<bool> CanApproveWorkflowAsync(string userId, string workflowId, string currentStage);
    Task<bool> CanAccessConfidentialDataAsync(string userId);
    
    // Workflow Permissions
    Task<WorkflowPermissions> GetWorkflowPermissionsAsync(string userId, string requestId);
}

public interface IUserService
{
    Task<User> GetUserByIdAsync(string userId);
    Task<User> GetUserByEmailAsync(string email);
    Task<List<User>> GetUsersByRoleAsync(string role);
}
```

### 2.2 Role Definitions (from UNESCO)

```csharp
public static class AppRoles
{
    // UNESCO roles adapted to our system
    public const string Administrator = "Administrator";
    public const string VendorUnitManager = "VendorUnitManager";  // UNESCO: VendorUnit
    public const string FinanceManager = "FinanceManager";        // UNESCO: BFM (Budget & Finance)
    public const string Requestor = "Requestor";                  // Can create requests
    public const string Viewer = "Viewer";                        // Read-only access
    public const string SapIntegrator = "SapIntegrator";         // SAP posting permissions
}

public class WorkflowPermissions
{
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanWithdraw { get; set; }
    public bool CanComment { get; set; }
    public bool CanReassign { get; set; }
}
```

### 2.3 Simulation Implementation

```csharp
public class AuthorizationSimulationService : IAuthorizationService
{
    private readonly ILogger<AuthorizationSimulationService> _logger;
    private readonly Dictionary<string, List<string>> _mockUserRoles;

    public AuthorizationSimulationService()
    {
        // Seed mock users with roles
        _mockUserRoles = new Dictionary<string, List<string>>
        {
            ["admin@test.com"] = new() { AppRoles.Administrator, AppRoles.VendorUnitManager },
            ["vendor.manager@test.com"] = new() { AppRoles.VendorUnitManager },
            ["finance@test.com"] = new() { AppRoles.FinanceManager },
            ["user@test.com"] = new() { AppRoles.Requestor },
        };
    }

    public Task<bool> HasRoleAsync(string userId, string role)
    {
        _logger.LogInformation("SIMULATION: Checking if {UserId} has role {Role}", userId, role);
        
        if (_mockUserRoles.TryGetValue(userId, out var roles))
        {
            return Task.FromResult(roles.Contains(role));
        }
        return Task.FromResult(false);
    }

    public Task<WorkflowPermissions> GetWorkflowPermissionsAsync(string userId, string requestId)
    {
        _logger.LogInformation("SIMULATION: Getting workflow permissions for {UserId} on {RequestId}", 
            userId, requestId);
        
        var permissions = new WorkflowPermissions
        {
            CanView = true,
            CanEdit = HasRoleAsync(userId, AppRoles.Requestor).Result,
            CanApprove = HasRoleAsync(userId, AppRoles.VendorUnitManager).Result ||
                        HasRoleAsync(userId, AppRoles.FinanceManager).Result,
            CanDelete = HasRoleAsync(userId, AppRoles.Administrator).Result,
            CanComment = true
        };

        return Task.FromResult(permissions);
    }
}
```

### 2.4 Real Implementation (Future)

```csharp
public class AzureAdAuthorizationService : IAuthorizationService
{
    private readonly IGraphServiceClient _graphClient;
    
    // Query Azure AD groups
    // Check group membership
    // Use Azure AD roles and claims
}
```

---

## 3. Master Data Services

### 3.1 Interface Definition

```csharp
public interface IMasterDataService
{
    // Countries
    Task<List<Country>> GetCountriesAsync();
    Task<Country> GetCountryByCodeAsync(string code);
    
    // Account Groups
    Task<List<AccountGroup>> GetAccountGroupsAsync(string companyCode, string vendorType);
    Task<AccountGroup> GetAccountGroupByCodeAsync(string code);
    
    // Currencies
    Task<List<Currency>> GetCurrenciesAsync();
    Task<Currency> GetCurrencyByCodeAsync(string code);
    
    // Languages
    Task<List<Language>> GetLanguagesAsync();
    
    // Companies (UNES, IIEP, UBO, UIS)
    Task<List<CompanyCode>> GetCompanyCodesAsync();
    
    // Bank Country Configuration
    Task<BankCountryConfig> GetBankCountryConfigAsync(string countryCode);
}

public class Country
{
    public string Code { get; set; }          // ISO2: "FR"
    public string Name { get; set; }          // "France"
    public string Iso3 { get; set; }          // "FRA"
    public string PhoneCode { get; set; }     // "+33"
    public bool Active { get; set; }
    public string Region { get; set; }        // "Europe", "North America", etc.
}

public class BankCountryConfig
{
    public string CountryCode { get; set; }
    public bool RequiresIban { get; set; }
    public bool RequiresSwift { get; set; }
    public bool RequiresRoutingNumber { get; set; }
    public bool RequiresAccountNumber { get; set; }
    public int? IbanLength { get; set; }
    public string[] AllowedPaymentFormats { get; set; }  // ["SEPA", "WIRE"]
    public Dictionary<string, string> FieldLabels { get; set; }
}
```

### 3.2 Simulation Implementation

```csharp
public class MasterDataSimulationService : IMasterDataService
{
    private readonly List<Country> _countries;
    private readonly List<AccountGroup> _accountGroups;
    private readonly List<Currency> _currencies;
    private readonly Dictionary<string, BankCountryConfig> _bankConfigs;

    public MasterDataSimulationService()
    {
        // Seed data from UNESCO patterns
        SeedCountries();
        SeedAccountGroups();
        SeedCurrencies();
        SeedBankConfigurations();
    }

    private void SeedCountries()
    {
        _countries = new List<Country>
        {
            new() { Code = "FR", Name = "France", Iso3 = "FRA", PhoneCode = "+33", Active = true, Region = "Europe" },
            new() { Code = "US", Name = "United States", Iso3 = "USA", PhoneCode = "+1", Active = true, Region = "North America" },
            new() { Code = "DE", Name = "Germany", Iso3 = "DEU", PhoneCode = "+49", Active = true, Region = "Europe" },
            new() { Code = "AR", Name = "Argentina", Iso3 = "ARG", PhoneCode = "+54", Active = true, Region = "Latin America" },
            new() { Code = "GB", Name = "United Kingdom", Iso3 = "GBR", PhoneCode = "+44", Active = true, Region = "Europe" },
            new() { Code = "ES", Name = "Spain", Iso3 = "ESP", PhoneCode = "+34", Active = true, Region = "Europe" },
            new() { Code = "IT", Name = "Italy", Iso3 = "ITA", PhoneCode = "+39", Active = true, Region = "Europe" },
            new() { Code = "BR", Name = "Brazil", Iso3 = "BRA", PhoneCode = "+55", Active = true, Region = "Latin America" },
            new() { Code = "MX", Name = "Mexico", Iso3 = "MEX", PhoneCode = "+52", Active = true, Region = "Latin America" },
            new() { Code = "CA", Name = "Canada", Iso3 = "CAN", PhoneCode = "+1", Active = true, Region = "North America" },
            // ... ~195 countries total
        };
    }

    private void SeedAccountGroups()
    {
        _accountGroups = new List<AccountGroup>
        {
            new() 
            { 
                Code = "INDV", 
                Description = "Individual - Physical Person",
                VendorType = "Physical",
                ReconciliationAccount = "1110010000",
                PaymentTerms = "Z001",
                TaxCategory = "Standard"
            },
            new() 
            { 
                Code = "SCSA", 
                Description = "SC - Staff Contract Holder",
                VendorType = "Physical",
                ReconciliationAccount = "1110020000",
                PaymentTerms = "Z002",
                TaxCategory = "Staff"
            },
            new() 
            { 
                Code = "HQSU", 
                Description = "Supplier - Goods & Services",
                VendorType = "Company",
                ReconciliationAccount = "1110030000",
                PaymentTerms = "Z030",
                TaxCategory = "Standard"
            },
            // ... more account groups
        };
    }

    private void SeedBankConfigurations()
    {
        _bankConfigs = new Dictionary<string, BankCountryConfig>
        {
            // SEPA Countries (France)
            ["FR"] = new()
            {
                CountryCode = "FR",
                RequiresIban = true,
                RequiresSwift = true,
                RequiresAccountNumber = false,
                IbanLength = 27,
                AllowedPaymentFormats = new[] { "SEPA", "WIRE" },
                FieldLabels = new()
                {
                    ["iban"] = "IBAN",
                    ["swift"] = "BIC/SWIFT",
                    ["bankName"] = "Bank Name"
                }
            },
            // United States
            ["US"] = new()
            {
                CountryCode = "US",
                RequiresIban = false,
                RequiresSwift = true,
                RequiresRoutingNumber = true,
                RequiresAccountNumber = true,
                AllowedPaymentFormats = new[] { "ACH", "WIRE" },
                FieldLabels = new()
                {
                    ["routingNumber"] = "ABA Routing Number",
                    ["accountNumber"] = "Account Number",
                    ["swift"] = "SWIFT (for international)",
                    ["accountType"] = "Account Type (Checking/Savings)"
                }
            },
            // ... configurations for all countries
        };
    }

    public Task<List<Country>> GetCountriesAsync()
    {
        return Task.FromResult(_countries);
    }

    public Task<BankCountryConfig> GetBankCountryConfigAsync(string countryCode)
    {
        if (_bankConfigs.TryGetValue(countryCode, out var config))
            return Task.FromResult(config);
        
        return Task.FromResult<BankCountryConfig>(null);
    }
}
```

### 3.3 Real Implementation (Future)

```csharp
public class DatabaseMasterDataService : IMasterDataService
{
    private readonly SqlDbContext _dbContext;
    private readonly IMemoryCache _cache;
    
    // Read from SQL Server master data tables
    // Cache with 24-hour expiration
    // Support for localization (i18n)
}
```

---

## 4. Workflow / Approval Services

### 4.1 Interface Definition

```csharp
public interface IWorkflowService
{
    // Workflow Status
    Task<WorkflowStatus> GetWorkflowStatusAsync(string requestId);
    
    // Workflow Actions
    Task<WorkflowActionResult> SubmitAsync(string requestId, string submittedBy);
    Task<WorkflowActionResult> ApproveAsync(string requestId, string approvedBy, string comments);
    Task<WorkflowActionResult> RejectAsync(string requestId, string rejectedBy, string reason);
    Task<WorkflowActionResult> WithdrawAsync(string requestId, string withdrawnBy);
    Task<WorkflowActionResult> ReassignAsync(string requestId, string assignedTo, string assignedBy);
    
    // Workflow History
    Task<List<WorkflowHistoryEntry>> GetHistoryAsync(string requestId);
    
    // Queue Management
    Task<List<WorkflowQueueItem>> GetMyWorkItemsAsync(string userId);
    Task<List<WorkflowQueueItem>> GetQueueAsync(string queueName);
}

public class WorkflowStatus
{
    public string RequestId { get; set; }
    public string Status { get; set; }               // Draft, Submitted, VendorUnitReview, etc.
    public string CurrentStep { get; set; }
    public string CurrentAssignee { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? SlaDeadline { get; set; }
    public int ProgressPercentage { get; set; }
    public List<WorkflowStep> Steps { get; set; }
}

public class WorkflowStep
{
    public string StepName { get; set; }
    public string Status { get; set; }              // Completed, InProgress, Pending
    public string AssignedTo { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CompletedBy { get; set; }
    public string Duration { get; set; }
}
```

### 4.2 Workflow Stages (from UNESCO)

```csharp
public static class WorkflowStages
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string VendorUnitReview = "VendorUnitReview";
    public const string FinanceApproval = "FinanceApproval";
    public const string SapPosting = "SapPosting";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";
    public const string Withdrawn = "Withdrawn";
}

public static class WorkflowQueues
{
    public const string VendorUnit = "VendorUnit";
    public const string Finance = "Finance";
    public const string SapIntegration = "SapIntegration";
}
```

### 4.3 Simulation Implementation

```csharp
public class WorkflowSimulationService : IWorkflowService
{
    private readonly ILogger<WorkflowSimulationService> _logger;
    private readonly Dictionary<string, WorkflowState> _workflows;

    public async Task<WorkflowActionResult> SubmitAsync(string requestId, string submittedBy)
    {
        _logger.LogInformation("SIMULATION: Submitting request {RequestId} by {User}", 
            requestId, submittedBy);

        if (!_workflows.TryGetValue(requestId, out var workflow))
        {
            return new WorkflowActionResult
            {
                Success = false,
                Error = "Request not found"
            };
        }

        // Simulate workflow transition
        workflow.Status = WorkflowStages.VendorUnitReview;
        workflow.CurrentStep = WorkflowStages.VendorUnitReview;
        workflow.CurrentAssignee = "vendor.unit@organization.org";
        workflow.SubmittedAt = DateTime.UtcNow;
        workflow.SlaDeadline = DateTime.UtcNow.AddDays(2);

        // Add history entry
        workflow.History.Add(new WorkflowHistoryEntry
        {
            Action = "Submit",
            PerformedBy = submittedBy,
            PerformedAt = DateTime.UtcNow,
            FromStatus = WorkflowStages.Draft,
            ToStatus = WorkflowStages.VendorUnitReview,
            Comments = "Request submitted for approval"
        });

        return new WorkflowActionResult
        {
            Success = true,
            NewStatus = workflow.Status,
            Message = "Request submitted successfully to Vendor Unit"
        };
    }

    public async Task<List<WorkflowQueueItem>> GetMyWorkItemsAsync(string userId)
    {
        _logger.LogInformation("SIMULATION: Getting work items for {UserId}", userId);

        // Simulate returning work items assigned to user
        var workItems = _workflows.Values
            .Where(w => w.CurrentAssignee == userId || 
                       IsUserInApprovalQueue(userId, w.CurrentStep))
            .Select(w => new WorkflowQueueItem
            {
                RequestId = w.RequestId,
                VendorName = w.VendorName,
                CurrentStep = w.CurrentStep,
                SubmittedBy = w.SubmittedBy,
                SubmittedAt = w.SubmittedAt,
                SlaDeadline = w.SlaDeadline,
                Priority = CalculatePriority(w)
            })
            .OrderByDescending(w => w.Priority)
            .ThenBy(w => w.SubmittedAt)
            .ToList();

        return workItems;
    }
}
```

### 4.4 Real Implementation (Future)

```csharp
public class DatabaseWorkflowService : IWorkflowService
{
    private readonly SqlDbContext _dbContext;
    private readonly IServiceBusPublisher _serviceBus;
    
    // Store workflow state in SQL Server
    // Publish events to Azure Service Bus
    // SLA monitoring and alerts
}
```

---

## 5. Email / Notification Services

### 5.1 Interface Definition

```csharp
public interface IEmailService
{
    Task<EmailResult> SendEmailAsync(EmailMessage message);
    Task<EmailResult> SendBatchAsync(List<EmailMessage> messages);
    Task<EmailResult> SendTemplatedEmailAsync(string templateName, string to, object data);
}

public interface INotificationService
{
    // Email Notifications
    Task SendVendorSubmittedNotificationAsync(string requestId, string submittedBy);
    Task SendApprovalRequestNotificationAsync(string requestId, string approverEmail);
    Task SendApprovedNotificationAsync(string requestId, string requestorEmail);
    Task SendRejectedNotificationAsync(string requestId, string requestorEmail, string reason);
    
    // System Notifications (in-app)
    Task SendSystemNotificationAsync(string userId, string message, string type);
}

public class EmailMessage
{
    public string To { get; set; }
    public string[]  Cc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; }
    public List<EmailAttachment> Attachments { get; set; }
}
```

### 5.2 Simulation Implementation

```csharp
public class EmailSimulationService : IEmailService
{
    private readonly ILogger<EmailSimulationService> _logger;
    private readonly List<EmailMessage> _sentEmails;  // For testing

    public Task<EmailResult> SendEmailAsync(EmailMessage message)
    {
        _logger.LogInformation("SIMULATION: Sending email to {To}", message.To);
        _logger.LogInformation("  Subject: {Subject}", message.Subject);
        _logger.LogInformation("  Body: {Body}", 
            message.Body.Substring(0, Math.Min(100, message.Body.Length)));

        // Store for test verification
        _sentEmails.Add(message);

        // Simulate email sent
        return Task.FromResult(new EmailResult
        {
            Success = true,
            MessageId = Guid.NewGuid().ToString(),
            SentAt = DateTime.UtcNow
        });
    }

    public List< EmailMessage> GetSentEmails() => _sentEmails;  // For testing
}

public class NotificationSimulationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationSimulationService> _logger;

    public async Task SendVendorSubmittedNotificationAsync(string requestId, string submittedBy)
    {
        _logger.LogInformation("SIMULATION: Sending vendor submitted notification for {RequestId}", 
            requestId);

        await _emailService.SendEmailAsync(new EmailMessage
        {
            To = "vendor.unit@organization.org",
            Subject = $"New Vendor Request: {requestId}",
            Body = $@"
                <h2>New Vendor Request Submitted</h2>
                <p>A new vendor request has been submitted and requires your review.</p>
                <ul>
                    <li>Request ID: {requestId}</li>
                    <li>Submitted By: {submittedBy}</li>
                    <li>Submitted At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                </ul>
                <p><a href='https://vendor-portal/requests/{requestId}'>Review Request</a></p>
            ",
            IsHtml = true
        });
    }
}
```

### 5.3 Real Implementation (Future)

```csharp
public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    
    // SendGrid API integration
    // Template management
    // Delivery tracking
}
```

---

## 6. File Storage Services

### 6.1 Interface Definition

```csharp
public interface IFileStorageService
{
    Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType, FileMetadata metadata);
    Task<Stream> DownloadFileAsync(string fileId);
    Task<bool> DeleteFileAsync(string fileId);
    Task<FileMetadata> GetFileMetadataAsync(string fileId);
    Task<List<FileMetadata>> GetFilesByRequestIdAsync(string requestId);
}

public class FileMetadata
{
    public string FileId { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string DocumentType { get; set; }        // "IdentificationID", "BankCertificate"
    public string RequestId { get; set; }
    public bool Confidential { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string VirusScanStatus { get; set; }     // "Pending", "Clean", "Infected"
    public string StorageLocation { get; set; }
}
```

### 6.2 Simulation Implementation

```csharp
public class FileStorageSimulationService : IFileStorageService
{
    private readonly ILogger<FileStorageSimulationService> _logger;
    private readonly string _tempStoragePath;
    private readonly Dictionary<string, FileMetadata> _fileMetadata;

    public FileStorageSimulationService(IConfiguration configuration)
    {
        _tempStoragePath = Path.Combine(Path.GetTempPath(), "vendor-mdm-simulation-files");
        Directory.CreateDirectory(_tempStoragePath);
        _fileMetadata = new Dictionary<string, FileMetadata>();
    }

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        FileMetadata metadata)
    {
        var fileId = $"att-{Guid.NewGuid()}";
        var filePath = Path.Combine(_tempStoragePath, fileId);

        _logger.LogInformation("SIMULATION: Uploading file {FileName} to {Path}", 
            fileName, filePath);

        // Save to temp file system
        using (var fileStreamOut = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileStreamOut);
        }

        // Store metadata
        metadata.FileId = fileId;
        metadata.FileName = fileName;
        metadata.FileSize = fileStream.Length;
        metadata.ContentType = contentType;
        metadata.StorageLocation = filePath;
        metadata.UploadedAt = DateTime.UtcNow;
        metadata.VirusScanStatus = "Clean";  // Simulate instant scan

        _fileMetadata[fileId] = metadata;

        return new FileUploadResult
        {
            Success = true,
            FileId = fileId,
            FileSize = metadata.FileSize,
            VirusScanStatus = "Clean"
        };
    }

    public async Task<Stream> DownloadFileAsync(string fileId)
    {
        _logger.LogInformation("SIMULATION: Downloading file {FileId}", fileId);

        if (!_fileMetadata.TryGetValue(fileId, out var metadata))
            throw new FileNotFoundException($"File {fileId} not found");

        return File.OpenRead(metadata.StorageLocation);
    }
}
```

### 6.3 Real Implementation (Future)

```csharp
public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly IVirusScanService _virusScanService;
    
    // Azure Blob Storage
    // Async virus scanning via Azure Defender
    // Metadata in SQL Server or Cosmos DB
}
```

---

## 7. Audit Trail Services

### 7.1 Interface Definition

```csharp
public interface IAuditService
{
    Task LogActionAsync(AuditEntry entry);
    Task<List<AuditEntry>> GetAuditTrailAsync(string requestId);
    Task<List<AuditEntry>> GetUserActivityAsync(string userId, DateTime from, DateTime to);
}

public class AuditEntry
{
    public string Id { get; set; }
    public string RequestId { get; set; }
    public string EntityType { get; set; }         // "VendorRequest", "WorkflowAction"
    public string EntityId { get; set; }
    public string Action { get; set; }              // "Create", "Update", "Submit", "Approve"
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public DateTime Timestamp { get; set; }
    public string FromStatus { get; set; }
    public string ToStatus { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public Dictionary<string, object> ChangeSet { get; set; }  // What changed
    public string Comments { get; set; }
}
```

### 7.2 Simulation Implementation

```csharp
public class AuditSimulationService : IAuditService
{
    private readonly ILogger<AuditSimulationService> _logger;
    private readonly List<AuditEntry> _auditLog;

    public Task LogActionAsync(AuditEntry entry)
    {
        entry.Id = Guid.NewGuid().ToString();
        entry.Timestamp = DateTime.UtcNow;

        _logger.LogInformation("AUDIT: {Action} by {User} on {Entity} {EntityId}",
            entry.Action, entry.UserEmail, entry.EntityType, entry.EntityId);

        _auditLog.Add(entry);
        return Task.CompletedTask;
    }

    public Task<List<AuditEntry>> GetAuditTrailAsync(string requestId)
    {
        var entries = _auditLog
            .Where(e => e.RequestId == requestId)
            .OrderBy(e => e.Timestamp)
            .ToList();

        return Task.FromResult(entries);
    }
}
```

### 7.3 Real Implementation (Future)

```csharp
public class DatabaseAuditService : IAuditService
{
    private readonly SqlDbContext _dbContext;
    private readonly IEventPublisher _eventPublisher;
    
    // Write to dedicated audit table
    // Publish to event stream for analytics
    // Support for compliance reporting
}
```

---

## 8. Service Registration & Configuration

### 8.1 appsettings.json

```json
{
  "Services": {
    "Mode": "Simulation",  // "Simulation" or "Production"
    
    "Sap": {
      "UseSimulation": true,
      "SimulateLatency": false,
      "LatencyMs": { "Min": 100, "Max": 500 }
    },
    
    "Authorization": {
      "UseSimulation": true,
      "Provider": "AzureAd"  // For future
    },
    
    "Email": {
      "UseSimulation": true,
      "Provider": "SendGrid",  // For future
      "LogEmailsToConsole": true
    },
    
    "FileStorage": {
      "UseSimulation": true,
      "Provider": "AzureBlob",  // For future
      "TempPath": "/tmp/vendor-mdm-files"
    },
    
    "Workflow": {
      "UseSimulation": true,
      "EnableSlaMonitoring": true
    }
  }
}
```

### 8.2 Program.cs Registration

```csharp
var serviceMode = builder.Configuration.GetValue<string>("Services:Mode");
var useSimulation = serviceMode == "Simulation";

// SAP Services
if (builder.Configuration.GetValue<bool>("Services:Sap:UseSimulation"))
{
    builder.Services.AddScoped<ISapVendorService, SapVendorSimulationService>();
    builder.Services.AddSingleton<ISapDataStore, InMemorySapDataStore>();
    builder.Services.AddScoped<ILevenshteinMatcher, LevenshteinMatcher>();
}
else
{
    builder.Services.AddScoped<ISapVendorService, SapVendorRfcService>();
    builder.Services.AddScoped<ISapConnectionPool, SapConnectionPool>();
}

// Authorization Services
if (builder.Configuration.GetValue<bool>("Services:Authorization:UseSimulation"))
{
    builder.Services.AddScoped<IAuthorizationService, AuthorizationSimulationService>();
    builder.Services.AddScoped<IUserService, UserSimulationService>();
}
else
{
    builder.Services.AddScoped<IAuthorizationService, AzureAdAuthorizationService>();
    builder.Services.AddScoped<IUserService, GraphUserService>();
}

// Master Data Services
builder.Services.AddScoped<IMasterDataService, MasterDataSimulationService>();
// Note: Master data will likely always be from DB, so simulation is just for initial dev

// Email Services
if (builder.Configuration.GetValue<bool>("Services:Email:UseSimulation"))
{
    builder.Services.AddSingleton<IEmailService, EmailSimulationService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
}

// File Storage Services
if (builder.Configuration.GetValue<bool>("Services:FileStorage:UseSimulation"))
{
    builder.Services.AddScoped<IFileStorageService, FileStorageSimulationService>();
}
else
{
    builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
}

// Workflow Services
if (builder.Configuration.GetValue<bool>("Services:Workflow:UseSimulation"))
{
    builder.Services.AddSingleton<IWorkflowService, WorkflowSimulationService>();
}
else
{
    builder.Services.AddScoped<IWorkflowService, DatabaseWorkflowService>();
}

// Notification Services (depends on Email)
builder.Services.AddScoped<INotificationService, NotificationService>();

// Audit Services
builder.Services.AddScoped<IAuditService, AuditSimulationService>();
```

---

## 9. Implementation Phases

### Phase 1: Core Simulation (Week 1)
- [ ] SAP Vendor Services (Search, Get, Update, Validation)
- [ ] Master Data Services (Countries, Currencies, Account Groups)
- [ ] Basic RBAC (role checking)
- [ ] Audit logging to console

### Phase 2: Workflow & Notifications (Week 2)
- [ ] Workflow state management
- [ ] Approval workflow simulation
- [ ] Email simulation service
- [ ] Notification templates

### Phase 3: File Management (Week 3)
- [ ] File upload/download simulation
- [ ] Document type management
- [ ] Confidential data handling

### Phase 4: Integration & Testing (Week 4)
- [ ] Full end-to-end vendor onboarding flow
- [ ] Unit tests for all services
- [ ] Integration tests
- [ ] Swagger documentation
- [ ] Postman collection

### Phase 5: Real Implementation Prep (Future)
- [ ] SAP NCo integration
- [ ] Azure AD integration
- [ ] Azure Blob Storage
- [ ] SendGrid integration
- [ ] Service Bus for workflows

---

## 10. Testing Strategy

### 10.1 Service-Level Tests

```csharp
public class SapVendorServiceTests
{
    [Fact]
    public async Task SearchVendors_WithValidCriteria_ReturnsMatches()
    {
        // Given
        var service = new SapVendorSimulationService(...);
        
        // When
        var result = await service.SearchVendorsAsync(new VendorSearchRequest
        {
            FamilyName = "Smith",
            SearchThreshold = 0.75
        });
        
        // Then
        Assert.True(result.DuplicatesFound);
        Assert.All(result.Vendors, v => Assert.True(v.MatchScore >= 0.75));
    }
}

public class WorkflowServiceTests
{
    [Fact]
    public async Task SubmitRequest_ValidRequest_TransitionsToReview()
    {
        // Test workflow state transitions
    }
    
    [Fact]
    public async Task ApproveRequest_ByAuthorizedUser_Succeeds()
    {
        // Test authorization + workflow
    }
}
```

### 10.2 Integration Tests

```csharp
public class VendorOnboardingFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CompleteOnboardingFlow_EndToEnd_CreatesVendor()
    {
        // 1. Search for duplicates
        // 2. Create vendor request
        // 3. Upload documents
        // 4. Submit for approval
        // 5. Approve (as VendorUnit)
        // 6. Approve (as Finance)
        // 7. Post to SAP (simulation)
        // 8. Verify vendor created
    }
}
```

---

## 11. Migration Strategy

When moving from simulation to real implementations:

1. **Update Configuration** - Change `Services:Mode` to `Production`
2. **Enable Real Services** - Set `UseSimulation: false` for each service
3. **No Code Changes** - Controllers and business logic remain unchanged
4. **Test Incrementally** - Migrate one service at a time
5. **Maintain Simulation** - Keep simulation code for local dev and testing

---

## Success Criteria

- [ ] All controller endpoints work with simulation services
- [ ] Complete vendor onboarding flow functional
- [ ] RBAC enforced on all protected operations
- [ ] Email notifications sent for all workflow events
- [ ] File upload/download working
- [ ] Audit trail captured for all actions
- [ ] Unit tests >80% coverage
- [ ] Integration tests for critical paths
- [ ] Swagger documentation complete
- [ ] Can toggle between simulation and real via config
