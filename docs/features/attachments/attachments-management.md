# Vendor Attachment Management System

## Architecture Overview

This document describes the **Gold Standard** attachment handling system implemented for the Vendor MDM Portal. The system uses **Direct Upload with SAS Tokens** (Azure's equivalent to AWS Presigned URLs), which is the industry best practice for secure, scalable file management.

### Key Principles

✅ **Private Storage** - Azure Blob Storage container is 100% private (no public access)  
✅ **Direct Upload** - Files upload directly from client to Azure, eliminating backend as proxy  
✅ **Temporary URLs** - SAS tokens expire after 5-15 minutes  
✅ **UUID-based Names** - Prevents enumeration attacks  
✅ **Malware Scanning** - Serverless Azure Function scans all uploads  
✅ **Metadata in Database** - References stored in JSONB `attributes` column  

---

## Architecture Diagram

```
┌─────────────┐
│   Client    │
│  (Browser)  │
└──────┬──────┘
       │
       │ 1. Request Upload Permission
       │    POST /api/attachments/request-upload
       │    { fileName, contentType, category, vendorId }
       ↓
┌─────────────────────┐
│   Backend API       │
│  (.NET Core)        │
└──────┬──────────────┘
       │
       │ 2. Generate SAS Token
       │    Returns: { sasUrl, blobName, expiresAt }
       │
┌──────┴──────────────┐
│ Client receives URL │
│ PUT directly to:    │
│ https://blob.../    │
│ {vendor}/{cat}/uuid │
└──────┬──────────────┘
       │
       │ 3. Direct Upload (bypasses backend)
       ↓
┌─────────────────────────────┐
│  Azure Blob Storage         │
│  Container: vendor-attach.. │
│  Access: Private            │
└──────┬──────────────────────┘
       │
       │ 4. Blob Created Event
       ↓
┌─────────────────────────────┐
│  Azure Function             │
│  BlobTrigger (Serverless)   │
│  - Scan for malware         │
│  - Update DB status         │
└──────┬──────────────────────┘
       │
       │ 5. Update Scan Status
       ↓
┌─────────────────────────────┐
│  SQL Database               │
│  VendorApplication.         │
│  Attributes.Attachments     │
└─────────────────────────────┘
```

---

## Data Model

### Database Storage (SQL Server)

Attachments metadata is stored in the `Attributes` JSONB column of `VendorApplication`:

```json
{
  "attachments": [
    {
      "fileName": "passport.pdf",
      "blobName": "vendor-123/identification/d3b07384-uuid.pdf",
      "contentType": "application/pdf",
      "sizeBytes": 524288,
      "uploadedAt": "2026-01-03T18:30:00Z",
      "category": "Identification",
      "uploadedBy": "user@example.com",
      "scanStatus": "clean"
    }
  ]
}
```

### Backend Models

**C# Classes** ([AttributeModels.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Shared/Models/AttributeModels.cs)):

```csharp
public class AttachmentMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty; // UUID-based
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? UploadedBy { get; set; }
    public string? ScanStatus { get; set; } // "pending_scan", "clean", "infected"
}
```

**TypeScript Interfaces** (Frontend):

```typescript
export interface AttachmentMetadata {
  fileName: string;
  blobName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: string;
  category: string;
  uploadedBy?: string;
  scanStatus?: 'pending_scan' | 'clean' | 'infected';
}
```

---

## API Endpoints

### 1. Request Upload Permission

**Endpoint**: `POST /api/attachments/request-upload`

**Purpose**: Generate a SAS token for direct upload to Azure Blob Storage.

**Request**:
```json
{
  "fileName": "passport.pdf",
  "contentType": "application/pdf",
  "category": "Identification",
  "sizeBytes": 524288,
  "vendorId": "vendor-123"
}
```

**Validation**:
- ✅ File type must be: PDF, JPG, PNG, DOCX
- ✅ File size must be ≤ 10MB
- ✅ Category must be valid: "Identification", "BusinessLicense", etc.

**Response**:
```json
{
  "sasUrl": "https://stvmdmdev.blob.core.windows.net/vendor-attachments/vendor-123/identification/d3b07384.pdf?sig=...&se=2026-01-03T18:35:00Z",
  "blobName": "vendor-123/identification/d3b07384-uuid.pdf",
  "expiresAt": "2026-01-03T18:35:00Z"
}
```

**Security**:
- SAS token expires in **5 minutes**
- Token grants **WRITE permission only**
- Blob name uses UUID to prevent guessing

---

### 2. Confirm Upload Success

**Endpoint**: `POST /api/attachments/confirm-upload`

**Purpose**: Store attachment metadata in database after successful upload.

**Request**:
```json
{
  "blobName": "vendor-123/identification/d3b07384-uuid.pdf",
  "vendorId": "vendor-123"
}
```

**Process**:
1. Verify blob exists in Azure Storage
2. Extract metadata from blob (size, content type)
3. Add to `VendorApplication.Attributes.Attachments` array
4. Set `scanStatus: "pending_scan"`

**Response**:
```json
{
  "success": true,
  "attachmentId": "d3b07384-uuid"
}
```

---

### 3. Get Download URL

**Endpoint**: `GET /api/attachments/{blobName}/download-url`

**Purpose**: Generate temporary download URL with SAS token.

**Authorization**: User must have permission to view the vendor.

**Response**:
```json
{
  "downloadUrl": "https://stvmdmdev.blob.core.windows.net/vendor-attachments/vendor-123/identification/d3b07384.pdf?sig=...&se=2026-01-03T18:45:00Z",
  "expiresAt": "2026-01-03T18:45:00Z",
  "fileName": "passport.pdf"
}
```

**Content-Disposition**:
- **Images** (JPG, PNG): `inline` → Browser preview
- **PDFs**: `inline` → Browser preview
- **Others**: `attachment` → Force download

**Security**:
- SAS token expires in **15 minutes**
- Token grants **READ permission only**

---

### 4. Delete Attachment

**Endpoint**: `DELETE /api/attachments/{blobName}`

**Purpose**: Soft-delete attachment (30-day retention).

**Process**:
1. Move blob to `deleted-blobs` container
2. Remove from `VendorApplication.Attributes.Attachments`
3. Azure soft delete retains for 30 days (automatic)

**Response**:
```json
{
  "success": true
}
```

---

## Malware Scanning (Azure Function)

### Function Configuration

**File**: [infrastructure/modules/malware-scan-function.bicep](file:///Users/jplopez/projects/vendor-mdm-portal/infrastructure/modules/malware-scan-function.bicep)

```bicep
resource scanFunction 'Microsoft.Web/sites@2023-01-01' = {
  name: 'func-vendor-malware-scan-${environmentName}'
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: functionAppPlan.id
    siteConfig: {
      appSettings: [
        { name: 'AzureWebJobsStorage', value: storageConnectionString }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
        { name: 'SQL_CONNECTION_STRING', value: '@Microsoft.KeyVault(...)' }
      ]
    }
  }
}
```

### Function Code

**Trigger**: Blob created in `vendor-attachments` container

```csharp
[Function("VendorAttachmentScanner")]
public async Task Run(
    [BlobTrigger("vendor-attachments/{blobName}")] Stream blobStream,
    string blobName)
{
    _logger.LogInformation($"Scanning blob: {blobName}");
    
    // 1. Scan blob with Azure Defender for Storage (if enabled)
    // OR integrate with ClamAV API / VirusTotal
    var scanResult = await _scanService.ScanBlobAsync(blobStream);
    
    // 2. Update VendorApplication.Attributes.Attachments[x].ScanStatus
    await _vendorService.UpdateAttachmentScanStatusAsync(
        blobName, 
        scanResult.IsClean ? "clean" : "infected"
    );
    
    // 3. If infected: quarantine and notify
    if (!scanResult.IsClean)
    {
        await _blobService.MoveBlobToQuarantineAsync(blobName);
        await _emailService.SendMalwareAlertAsync(blobName, scanResult);
    }
}
```

---

## Frontend Integration

### File Upload Component

**Component**: [FileUpload.tsx](file:///Users/jplopez/projects/vendor-mdm-portal/frontend/src/components/ui/FileUpload.tsx)

**Usage**:
```tsx
<FileUpload
  label="Identification Documents (Max 2)"
  category="Identification"
  maxFiles={2}
  accept="application/pdf,image/jpeg,image/png"
  onUploadComplete={(metadata) => {
    // Add to form state
    setValue('attributes.attachments', [...existingFiles, metadata]);
  }}
  onDelete={(blobName) => {
    // Remove from form state
    const filtered = existingFiles.filter(f => f.blobName !== blobName);
    setValue('attributes.attachments', filtered);
  }}
  existingFiles={watch('attributes.attachments')}
/>
```

### Upload Flow (Frontend)

```typescript
// 1. User selects file
const handleFileSelect = async (file: File) => {
  // 2. Request upload permission from backend
  const { sasUrl, blobName, expiresAt } = await api.post('/attachments/request-upload', {
    fileName: file.name,
    contentType: file.type,
    category: 'Identification',
    sizeBytes: file.size,
    vendorId: vendorId
  });
  
  // 3. Upload directly to Azure Blob Storage
  await fetch(sasUrl, {
    method: 'PUT',
    headers: { 'x-ms-blob-type': 'BlockBlob' },
    body: file
  });
  
  // 4. Confirm upload to backend
  const { attachmentId } = await api.post('/attachments/confirm-upload', {
    blobName,
    vendorId
  });
  
  // 5. Update UI
  setAttachments([...attachments, { fileName: file.name, blobName, ... }]);
};
```

---

## Security Best Practices

### ✅ Implemented

| Security Control | Implementation |
|------------------|----------------|
| **Private Bucket** | `publicAccess: 'None'` in storage.bicep |
| **UUID Blob Names** | `{vendorId}/{category}/{GUID}.{ext}` |
| **Short-lived Tokens** | 5 min upload, 15 min download |
| **File Type Whitelist** | PDF, JPG, PNG, DOCX only |
| **Size Limit** | 10MB maximum |
| **Malware Scanning** | Azure Function with BlobTrigger |
| **Soft Delete** | 30-day retention for recovery |
| **HTTPS Only** | `supportsHttpsTrafficOnly: true` |

### 🔒 Additional Recommendations (Production)

- **Azure Defender for Storage**: Enable for real-time threat detection
- **Content Scanning**: Integrate with Azure Content Moderator for sensitive data detection
- **Audit Logging**: Enable Azure Monitor logs for all blob operations
- **Geo-Redundancy**: Upgrade to GRS (Geo-Redundant Storage) for production
- **DDoS Protection**: Configure Azure Front Door with WAF

---

## Vendor Type-Specific Categories

| Vendor Type | Attachment Categories |
|-------------|----------------------|
| **Physical** | Identification (Max 2) |
| **Company** | BusinessLicense, TaxRegistration |
| **Meeting** | EventPermit, VenueLicense |
| **Participant** | Identification (Max 2) |

---

## Error Handling

### Common Error Scenarios

**1. File Too Large**
```json
{
  "error": "File size exceeds 10MB limit",
  "maxSize": 10485760,
  "actualSize": 15728640
}
```

**2. Invalid File Type**
```json
{
  "error": "File type not allowed",
  "allowedTypes": ["application/pdf", "image/jpeg", "image/png", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
  "actualType": "application/zip"
}
```

**3. SAS Token Expired**
```json
{
  "error": "Upload token expired",
  "expiredAt": "2026-01-03T18:35:00Z",
  "currentTime": "2026-01-03T18:36:00Z"
}
```

**4. Malware Detected**
```json
{
  "error": "File failed security scan",
  "scanStatus": "infected",
  "threatName": "EICAR-Test-File"
}
```

---

## Monitoring & Diagnostics

### Key Metrics to Monitor

1. **Upload Success Rate**: Should be > 95%
2. **SAS Token Expiration**: Track premature expiration errors
3. **Malware Detection Rate**: Should be low, alert if spike
4. **Storage Costs**: Monitor for unexpected growth
5. **Blob Lifecycle**: Ensure old blobs are cleaned up

### Azure Monitor Queries

**Failed Uploads (Last 24h)**:
```kusto
traces
| where timestamp > ago(24h)
| where message contains "Upload failed"
| summarize count() by bin(timestamp, 1h)
```

**Malware Detections**:
```kusto
traces
| where customDimensions.scanStatus == "infected"
| project timestamp, blobName, threatName
```

---

## Deployment Checklist

- [ ] Deploy storage.bicep module
- [ ] Configure Key Vault secret: `ConnectionStrings--BlobStorage`
- [ ] Grant App Service managed identity: Storage Blob Data Contributor role
- [ ] Deploy malware scanning Azure Function
- [ ] Configure CORS for Static Web App origin
- [ ] Test direct upload flow end-to-end
- [ ] Verify soft delete is enabled (30 days)
- [ ] Set up Azure Monitor alerts for malware detection

---

## References

- [Azure Blob Storage SAS Tokens](https://learn.microsoft.com/en-us/azure/storage/common/storage-sas-overview)
- [Azure Functions Blob Trigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-trigger)
- [storage.bicep](file:///Users/jplopez/projects/vendor-mdm-portal/infrastructure/modules/storage.bicep)
- [Implementation Plan](file:///Users/jplopez/.gemini/antigravity/brain/70825252-5e3c-4892-8afa-4abb9ad3d7d4/implementation_plan.md)
