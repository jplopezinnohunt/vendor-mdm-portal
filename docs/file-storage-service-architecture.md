# Canonical File Storage Service - Architecture & Implementation Plan

## Overview

Design and implement a canonical file storage service that supports **multiple apps and processes** with **per-process folder organization** in Azure Blob Storage. The service follows the **Mock/Real progressive rollout pattern** established for other integrations.

---

## MoUV Reference Analysis

### File Upload Pattern from UNESCO System

```
POST /Vendor/UploadFile

Features:
- Max 2 files for identification documents
- Accepted formats: PDF, JPG, PNG
- Max size: 10MB per file  
- Azure Blob Storage backend
- Virus scanning integration
- File metadata in SQL database
- Confidential data flagging
- Download/preview capabilities
```

### Key Observations

1. **Process-Specific Storage** - Files are associated with a specific entity (vendor request ID)
2. **Metadata Tracking** - File name, size, upload date, uploader, content type stored separately
3. **Security** - Virus scanning before acceptance, confidential flag
4. **Organization** - Logical grouping by request/entity type

---

## Architecture Design

### Folder Structure Strategy

```
Container: vendor-mdm-files
│
├── invitations/
│   ├── {invitation-id}/
│   │   ├── documents/
│   │   │   ├── identification-{guid}.pdf
│   │   │   └── bank-certificate-{guid}.pdf
│   │   └── attachments/
│   │       └── additional-{guid}.pdf
│
├── vendors/
│   ├── {vendor-id}/
│   │   ├── kyc/
│   │   │   ├── passport-{guid}.pdf
│   │   │   └── tax-id-{guid}.pdf
│   │   ├── bank/
│   │   │   └── bank-cert-{guid}.pdf
│   │   └── contracts/
│   │       └── contract-{guid}.pdf
│
├── workflows/
│   ├── {workflow-id}/
│   │   └── approvals/
│   │       └── approval-doc-{guid}.pdf
│
└── temp/
    └── {upload-session-{guid}.tmp

Naming Convention:
- {app}: invitations, vendors, workflows, master-data
- {process}: documents, kyc, bank, contracts, approvals
- {entity-id}: GUID or ID of parent entity
- {filename}: category-{guid}.extension
```

###  Storage Path Construction

```csharp
Path = {app}/{entity-id}/{process}/{category}-{guid}.{ext}

Examples:
- invitations/INV-2025-001/documents/passport-a1b2c3.pdf
- vendors/VEN-12345/bank/bank-certificate-d4e5f6.pdf
- workflows/WF-2025-100/approvals/approval-g7h8i9.pdf
```

---

## Service Interface Design

### Core Interface

```csharp
public interface IFileStorageService
{
    // Upload file
    Task<FileUploadResult> UploadFileAsync(FileUploadRequest request);
    
    // Download file
    Task<FileDownloadResult> DownloadFileAsync(string fileId);
    
    // Get file metadata
    Task<FileMetadata> GetFileMetadataAsync(string fileId);
    
    // List files for entity
    Task<List<FileMetadata>> ListFilesAsync(string app, string entityId, string? process = null);
    
    // Delete file
    Task<bool> DeleteFileAsync(string fileId);
    
    // Generate SAS URL for direct download
    Task<string> GenerateDownloadUrlAsync(string fileId, int expirationMinutes = 60);
    
    // Check file exists
    Task<bool> FileExistsAsync(string fileId);
}
```

### Models

```csharp
public class FileUploadRequest
{
    public string App { get; set; }              // "invitations", "vendors", etc.
    public string EntityId { get; set; }          // "INV-2025-001", "VEN-12345"
    public string Process { get; set; }           // "documents", "kyc", "bank"
    public string Category { get; set; }          // "passport", "bank-certificate"
    public Stream FileStream { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public bool IsConfidential { get; set; }
    public string UploadedBy { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FileId { get; set; }
    public string? StoragePath { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

public class FileMetadata
{
    public string FileId { get; set; }
    public string App { get; set; }
    public string EntityId { get; set; }
    public string Process { get; set; }
    public string Category { get; set; }
    public string FileName { get; set; }
    public string StoragePath { get; set; }
    public string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public bool IsConfidential { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; }
    public string? VirusScanStatus { get; set; }  // "Pending", "Clean", "Infected"
    public Dictionary<string, string> Metadata { get; set; }
}

public class FileDownloadResult
{
    public bool Success { get; set; }
    public Stream? FileStream { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

## Implementation 1: Mock Service (Filesystem-based)

### FileStorageSimulationService.cs

```csharp
public class FileStorageSimulationService : IFileStorageService
{
    private readonly ILogger<FileStorageSimulationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _basePath;
    private readonly Dictionary<string, FileMetadata> _mockMetadata;

    public FileStorageSimulationService(
        ILogger<FileStorageSimulationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _basePath = configuration["Services:FileStorage:MockSettings:TempPath"] 
            ?? "/tmp/vendor-mdm-files";
        _mockMetadata = new Dictionary<string, FileMetadata>();
        
        // Ensure base directory exists
        Directory.CreateDirectory(_basePath);
        _logger.LogInformation("MOCK FILE STORAGE: Using path {Path}", _basePath);
    }

    public async Task<FileUploadResult> UploadFileAsync(FileUploadRequest request)
    {
        _logger.LogInformation(
            "MOCK: Uploading file {FileName} for {App}/{EntityId}/{Process}",
            request.FileName, request.App, request.EntityId, request.Process);

        var result = new FileUploadResult();

        // Validate
        var validationErrors = ValidateUpload(request);
       if (validationErrors.Any())
        {
            result.Success = false;
            result.ValidationErrors = validationErrors;
            return result;
        }

        try
        {
            // Generate file ID and path
            var fileId = Guid.NewGuid().ToString();
            var storagePath = BuildStoragePath(
                request.App, request.EntityId, request.Process, 
                request.Category, fileId, Path.GetExtension(request.FileName));

            var fullPath = Path.Combine(_basePath, storagePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            // Save file
            using (var fileStream = File.Create(fullPath))
            {
                await request.FileStream.CopyToAsync(fileStream);
            }

            // Save metadata
            var metadata = new FileMetadata
            {
                FileId = fileId,
                App = request.App,
                EntityId = request.EntityId,
                Process = request.Process,
                Category = request.Category,
                FileName = request.FileName,
                StoragePath = storagePath,
                ContentType = request.ContentType,
                FileSizeBytes = request.FileSize,
                IsConfidential = request.IsConfidential,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = request.UploadedBy,
                VirusScanStatus = "Clean", // Mock always clean
                Metadata = request.Metadata ?? new()
            };
            _mockMetadata[fileId] = metadata;

            result.Success = true;
            result.FileId = fileId;
            result.StoragePath = storagePath;

            _logger.LogInformation("MOCK: File uploaded successfully: {FileId}", fileId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MOCK: Error uploading file");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<FileDownloadResult> DownloadFileAsync(string fileId)
    {
        _logger.LogInformation("MOCK: Downloading file {FileId}", fileId);

        if (!_mockMetadata.TryGetValue(fileId, out var metadata))
        {
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = "File not found"
            };
        }

        try
        {
            var fullPath = Path.Combine(_basePath, metadata.StoragePath);
            var fileStream = File.OpenRead(fullPath);

            return new FileDownloadResult
            {
                Success = true,
                FileStream = fileStream,
                FileName = metadata.FileName,
                ContentType = metadata.ContentType,
                FileSizeBytes = metadata.FileSizeBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MOCK: Error downloading file {FileId}", fileId);
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<FileMetadata> GetFileMetadataAsync(string fileId)
    {
        if (_mockMetadata.TryGetValue(fileId, out var metadata))
        {
            return Task.FromResult(metadata);
        }
        throw new FileNotFoundException($"File {fileId} not found");
    }

    public Task<List<FileMetadata>> ListFilesAsync(string app, string entityId, string? process = null)
    {
        var files = _mockMetadata.Values
            .Where(f => f.App == app && f.EntityId == entityId)
            .Where(f => process == null || f.Process == process)
            .OrderByDescending(f => f.UploadedAt)
            .ToList();

        return Task.FromResult(files);
    }

    public Task<bool> DeleteFileAsync(string fileId)
    {
        if (_mockMetadata.TryGetValue(fileId, out var metadata))
        {
            var fullPath = Path.Combine(_basePath, metadata.StoragePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            _mockMetadata.Remove(fileId);
            _logger.LogInformation("MOCK: File deleted {FileId}", fileId);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<string> GenerateDownloadUrlAsync(string fileId, int expirationMinutes = 60)
    {
        // Mock returns a simple URL
        var url = $"/api/files/download/{fileId}?mock=true&expires={expirationMinutes}";
        return Task.FromResult(url);
    }

    public Task<bool> FileExistsAsync(string fileId)
    {
        return Task.FromResult(_mockMetadata.ContainsKey(fileId));
    }

    private string BuildStoragePath(string app, string entityId, string process, 
        string category, string fileId, string extension)
    {
        // Pattern: {app}/{entityId}/{process}/{category}-{guid}.{ext}
        var fileName = $"{category}-{fileId}{extension}";
        return Path.Combine(app, entityId, process, fileName);
    }

    private List<string> ValidateUpload(FileUploadRequest request)
    {
        var errors = new List<string>();

        // Max file size (10MB default)
        var maxSize = _configuration.GetValue<long>("Services:FileStorage:MaxFileSizeBytes", 10485760);
        if (request.FileSize > maxSize)
        {
            errors.Add($"File size exceeds maximum of {maxSize / 1048576}MB");
        }

        // Allowed extensions
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var extension = Path.GetExtension(request.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            errors.Add($"File type {extension} not allowed");
        }

        // Required fields
        if (string.IsNullOrWhiteSpace(request.App))
            errors.Add("App is required");
        if (string.IsNullOrWhiteSpace(request.EntityId))
            errors.Add("EntityId is required");
        if (string.IsNullOrWhiteSpace(request.Process))
            errors.Add("Process is required");
        if (string.IsNullOrWhiteSpace(request.Category))
            errors.Add("Category is required");

        return errors;
    }
}
```

---

## Implementation 2: Real Service (Azure Blob Storage)

### FileStorageAzureBlobService.cs

```csharp
public class FileStorageAzureBlobService : IFileStorageService
{
    private readonly ILogger<FileStorageAzureBlobService> _logger;
    private readonly IConfiguration _configuration;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly SqlDbContext _dbContext;
    private readonly string _containerName;

    public FileStorageAzureBlobService(
        ILogger<FileStorageAzureBlobService> logger,
        IConfiguration configuration,
        BlobServiceClient blobServiceClient,
        SqlDbContext dbContext)
    {
        _logger = logger;
        _configuration = configuration;
        _blobServiceClient = blobServiceClient;
        _dbContext = dbContext;
        _containerName = configuration["Services:FileStorage:RealSettings:ContainerName"] 
            ?? "vendor-documents";
    }

    public async Task<FileUploadResult> UploadFileAsync(FileUploadRequest request)
    {
        _logger.LogInformation(
            "AZURE BLOB: Uploading file {FileName} for {App}/{EntityId}/{Process}",
            request.FileName, request.App, request.EntityId, request.Process);

        var result = new FileUploadResult();
        
        // Validate
        var validationErrors = ValidateUpload(request);
        if (validationErrors.Any())
        {
            result.Success = false;
            result.ValidationErrors = validationErrors;
            return result;
        }

        try
        {
            // Generate file ID and blob path
            var fileId = Guid.NewGuid().ToString();
            var blobPath = BuildBlobPath(
                request.App, request.EntityId, request.Process,
                request.Category, fileId, Path.GetExtension(request.FileName));

            // Get container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Upload blob
            var blobClient = containerClient.GetBlobClient(blobPath);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders 
                { 
                    ContentType = request.ContentType 
                },
                Metadata = request.Metadata ?? new(),
                Tags = new Dictionary<string, string>
                {
                    ["App"] = request.App,
                    ["EntityId"] = request.EntityId,
                    ["Process"] = request.Process,
                    ["Confidential"] = request.IsConfidential.ToString()
                }
            };

            await blobClient.UploadAsync(request.FileStream, uploadOptions);

            // Save metadata to database
            var fileMetadataEntity = new FileAttachment
            {
                FileId = fileId,
                App = request.App,
                EntityId = request.EntityId,
                Process = request.Process,
                Category = request.Category,
                FileName = request.FileName,
                StoragePath = blobPath,
                ContentType = request.ContentType,
                FileSizeBytes = request.FileSize,
                IsConfidential = request.IsConfidential,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = request.UploadedBy,
                VirusScanStatus = "Pending",
                MetadataJson = JsonSerializer.Serialize(request.Metadata ?? new())
            };

            _dbContext.FileAttachments.Add(fileMetadataEntity);
            await _dbContext.SaveChangesAsync();

            result.Success = true;
            result.FileId = fileId;
            result.StoragePath = blobPath;

            _logger.LogInformation("AZURE BLOB: File uploaded successfully: {FileId}", fileId);
            
            // TODO: Trigger virus scan asynchronously
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AZURE BLOB: Error uploading file");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<FileDownloadResult> DownloadFileAsync(string fileId)
    {
        _logger.LogInformation("AZURE BLOB: Downloading file {FileId}", fileId);

        var metadata = await _dbContext.FileAttachments
            .FirstOrDefaultAsync(f => f.FileId == fileId);

        if (metadata == null)
        {
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = "File not found"
            };
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(metadata.StoragePath);

            var download = await blobClient.DownloadAsync();

            return new FileDownloadResult
            {
                Success = true,
                FileStream = download.Value.Content,
                FileName = metadata.FileName,
                ContentType = metadata.ContentType,
                FileSizeBytes = metadata.FileSizeBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AZURE BLOB: Error downloading file {FileId}", fileId);
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> GenerateDownloadUrlAsync(string fileId, int expirationMinutes = 60)
    {
        var metadata = await _dbContext.FileAttachments
            .FirstOrDefaultAsync(f => f.FileId == fileId);

        if (metadata == null)
            throw new FileNotFoundException($"File {fileId} not found");

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(metadata.StoragePath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = metadata.StoragePath,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    // ... (similar implementations for other methods)
}
```

---

## Database Schema

### SQL Table: FileAttachments

```sql
CREATE TABLE FileAttachments (
    FileId NVARCHAR(50) PRIMARY KEY,
    App NVARCHAR(50) NOT NULL,
    EntityId NVARCHAR(100) NOT NULL,
    Process NVARCHAR(50) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    IsConfidential BIT NOT NULL DEFAULT 0,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedBy NVARCHAR(100) NOT NULL,
    VirusScanStatus NVARCHAR(20), -- 'Pending', 'Clean', 'Infected'
    MetadataJson NVARCHAR(MAX), -- JSONB for additional metadata
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    
    INDEX IX_FileAttachments_App_EntityId (App, EntityId),
    INDEX IX_FileAttachments_Process (Process),
    INDEX IX_FileAttachments_UploadedAt (UploadedAt DESC)
);
```

### EF Core Entity

```csharp
public class FileAttachment
{
    public string FileId { get; set; } = null!;
    public string App { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Process { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string StoragePath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public bool IsConfidential { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;
    public string? VirusScanStatus { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

## Configuration

### appsettings.json

```json
{
  "Services": {
    "FileStorage": {
      "UseMock": true,
      "RealProvider": "AzureBlob",
      "MaxFileSizeBytes": 10485760,
      "AllowedExtensions": [".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"],
      "MockSettings": {
        "TempPath": "/tmp/vendor-mdm-files"
      },
      "RealSettings": {
        "ContainerName": "vendor-documents",
        "EnableVirusScanning": true
      }
    }
  }
}
```

---

## Service Registration

### Program.cs

```csharp
// File Storage Service
var useFileStorageMock = builder.Configuration.GetValue<bool>("Services:FileStorage:UseMock", true);

if (useFileStorageMock)
{
    builder.Services.AddScoped<IFileStorageService, FileStorageSimulationService>();
    Console.WriteLine("✓ File Storage: MOCK (Local filesystem)");
}
else
{
    // Register Azure Blob Service Client
    builder.Services.AddSingleton(sp =>
    {
        var connectionString = builder.Configuration["Azure:Storage:ConnectionString"];
        return new BlobServiceClient(connectionString);
    });
    
    builder.Services.AddScoped<IFileStorageService, FileStorageAzureBlobService>();
    Console.WriteLine("✓ File Storage: REAL (Azure Blob Storage)");
}
```

---

## API Controller

### FilesController.cs

```csharp
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileStorageService fileStorage, ILogger<FilesController> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)] // 10MB
    public async Task<ActionResult<FileUploadResult>> UploadFile([FromForm] IFormFile file, 
        [FromForm] string app, [FromForm] string entityId, [FromForm] string process, 
        [FromForm] string category, [FromForm] bool isConfidential = false)
    {
        var request = new FileUploadRequest
        {
            App = app,
            EntityId = entityId,
            Process = process,
            Category = category,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            IsConfidential = isConfidential,
            UploadedBy = User.Identity?.Name ?? "Anonymous"
        };

        var result = await _fileStorage.UploadFileAsync(request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetFileMetadata), new { fileId = result.FileId }, result);
    }

    [HttpGet("{fileId}")]
    public async Task<ActionResult<FileMetadata>> GetFileMetadata(string fileId)
    {
        try
        {
            var metadata = await _fileStorage.GetFileMetadataAsync(fileId);
            return Ok(metadata);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        var result = await _fileStorage.DownloadFileAsync(fileId);
        
        if (!result.Success)
            return NotFound();
        
        return File(result.FileStream!, result.ContentType!, result.FileName);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<FileMetadata>>> ListFiles(
        [FromQuery] string app, [FromQuery] string entityId, [FromQuery] string? process = null)
    {
        var files = await _fileStorage.ListFilesAsync(app, entityId, process);
        return Ok(files);
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile(string fileId)
    {
        var deleted = await _fileStorage.DeleteFileAsync(fileId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("download-url/{fileId}")]
    public async Task<ActionResult<string>> GetDownloadUrl(string fileId, [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            var url = await _fileStorage.GenerateDownloadUrlAsync(fileId, expirationMinutes);
            return Ok(new { url });
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}
```

---

## Usage Examples

### Frontend (Invitation Upload)

```typescript
// Upload identification document for invitation
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('app', 'invitations');
formData.append('entityId', 'INV-2025-001');
formData.append('process', 'documents');
formData.append('category', 'passport');
formData.append('isConfidential', 'true');

const response = await fetch('/api/files/upload', {
  method: 'POST',
  body: formData
});

const result = await response.json();
console.log('File uploaded:', result.fileId);

// List all documents for invitation
const files = await fetch(
  '/api/files/list?app=invitations&entityId=INV-2025-001&process=documents'
).then(r => r.json());

// Download file
window.location.href = `/api/files/download/${fileId}`;
```

### Backend (Service Layer)

```csharp
// In InvitationService.cs
public async Task<string> AttachDocumentAsync(string invitationId, IFormFile file, string category)
{
    var uploadRequest = new FileUploadRequest
    {
        App = "invitations",
        EntityId = invitationId,
        Process = "documents",
        Category = category,
        FileStream = file.OpenReadStream(),
        FileName = file.FileName,
        ContentType = file.ContentType,
        FileSizeBytes = file.Length,
        IsConfidential = true,
        UploadedBy = _currentUser.Email
    };

    var result = await _fileStorage.UploadFileAsync(uploadRequest);
    
    if (!result.Success)
        throw new InvalidOperationException(result.ErrorMessage);
    
    return result.FileId;
}
```

---

## Testing Strategy

### Unit Tests

```csharp
[Fact]
public async Task UploadFile_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var service = CreateMockService();
    var request = CreateValidUploadRequest();

    // Act
    var result = await service.UploadFileAsync(request);

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.FileId);
}

[Fact]
public async Task UploadFile_ExceedsMaxSize_ReturnsValidationError()
{
    // Test file size limit
}

[Fact]
public async Task ListFiles_FilterByProcess_ReturnsCorrectFiles()
{
    // Test listing with filter
}
```

### Integration Tests

```csharp
[Fact]
public async Task EndToEnd_UploadDownloadDelete_WorksCorrectly()
{
    // Test complete lifecycle
}
```

---

## Migration Path

### Phase 1: Deploy with Mock (Now)
- Deploy to production with filesystem-based mock
- All file operations work locally
- No Azure Blob dependency

### Phase 2: Activate Azure Blob (When Ready)
```bash
# Update configuration
az webapp config appsettings set \
  --settings Services__FileStorage__UseMock=false \
             Services__FileStorage__RealSettings__ContainerName=vendor-documents

# Ensure storage account exists
az storage container create \
  --name vendor-documents \
  --account-name stvendormdmdev
```

### Phase 3: Data Migration (If Needed)
```bash
# Migrate existing mock files to Azure Blob
# Run migration script to copy from /tmp to blob storage
```

---

## Enhancements (Future)

1. **Virus Scanning** - Integrate Microsoft Defender for Cloud (Malware Scanning)
2. **Image Thumbnails** - Auto-generate thumbnails for images
3. **PDF Preview** - Generate preview images for PDFs
4. **File Versioning** - Keep multiple versions of same file
5. **Archival** - Move old files to cold storage tier
6. **Analytics** - Track upload/download metrics

---

## Success Criteria

- [x] Interface defined
- [x] Mock service (filesystem)
- [x] Real service (Azure Blob)
- [x] Database schema
- [x] API controller
- [ ] Frontend integration (invitation upload)
- [ ] Unit tests
- [ ] Integration tests
- [ ] Documentation
- [ ] Deploy with Mock to Dev

---

**This document provides the complete blueprint for canonical file storage service following our established Mock/Real pattern.**
