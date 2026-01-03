using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models.FileStorage;

namespace VendorMdm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentController : ControllerBase
{
    private readonly ILogger<AttachmentController> _logger;
    private readonly FileStorageAzureBlobService _blobStorageService;

    public AttachmentController(
        ILogger<AttachmentController> logger,
        FileStorageAzureBlobService blobStorageService)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
    }

    /// <summary>
    /// Request upload permission for a new attachment (Gold Standard Direct Upload pattern)
    /// Generates a SAS token for direct upload to Azure Blob Storage
    /// </summary>
    [HttpPost("request-upload")]
    public async Task<IActionResult> RequestUpload([FromBody] AttachmentUploadRequest request)
    {
        try
        {
            // Validate file type
            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

            if (!allowedTypes.Contains(request.ContentType.ToLower()))
            {
                return BadRequest(new
                {
                    error = "File type not allowed",
                    allowedTypes,
                    actualType = request.ContentType
                });
            }

            // Validate file size (10MB max)
            const long maxSize = 10 * 1024 * 1024;
            if (request.SizeBytes > maxSize)
            {
                return BadRequest(new
                {
                    error = "File size exceeds 10MB limit",
                    maxSize,
                    actualSize = request.SizeBytes
                });
            }

            // Generate unique blob name: {vendorId}/{category}/{GUID}.{ext}
            var extension = Path.GetExtension(request.FileName);
            var blobName = $"{request.VendorId}/{request.Category}/{Guid.NewGuid()}{extension}";

            // Generate SAS token (5 min expiry for upload)
            var sasUrl = await _blobStorageService.GenerateUploadSasTokenAsync(blobName, expirationMinutes: 5);
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

            _logger.LogInformation($"Generated upload SAS token for: {blobName}");

            return Ok(new
            {
                sasUrl,
                blobName,
                expiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upload SAS token");
            return StatusCode(500, new { error = "Failed to generate upload permission" });
        }
    }

    /// <summary>
    /// Confirm successful upload and store metadata in database
    /// </summary>
    [HttpPost("confirm-upload")]
    public async Task<IActionResult> ConfirmUpload([FromBody] AttachmentConfirmRequest request)
    {
        try
        {
            // Verify blob exists in storage
            var exists = await _blobStorageService.FileExistsAsync(request.BlobName);
            if (!exists)
            {
                return NotFound(new { error = "Blob not found in storage" });
            }

            // Get blob metadata
            var metadata = await _blobStorageService.GetFileMetadataAsync(request.BlobName);

            _logger.LogInformation($"Upload confirmed for: {request.BlobName}");

            // Return attachment metadata for client to store in form state
            return Ok(new
            {
                success = true,
                attachmentId = Guid.NewGuid(), // In production, this would be stored in DB
                metadata = new
                {
                    fileName = metadata.FileName,
                    blobName = request.BlobName,
                    contentType = metadata.ContentType,
                    sizeBytes = metadata.FileSizeBytes,
                    uploadedAt = metadata.UploadedAt,
                    category = metadata.Category,
                    scanStatus = "pending_scan"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm upload");
            return StatusCode(500, new { error = "Failed to confirm upload" });
        }
    }

    /// <summary>
    /// Generate temporary download URL with SAS token
    /// </summary>
    [HttpGet("{*blobName}/download-url")]
    public async Task<IActionResult> GetDownloadUrl(string blobName)
    {
        try
        {
            // TODO: Validate user has permission to access this vendor's attachments
            
            // Check if blob exists
            var exists = await _blobStorageService.FileExistsAsync(blobName);
            if (!exists)
            {
                return NotFound(new { error = "Attachment not found" });
            }

            // Get metadata for filename
            var metadata = await _blobStorageService.GetFileMetadataAsync(blobName);

            // Generate SAS token (15 min expiry for download)
            var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(blobName, expirationMinutes: 15);
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

            _logger.LogInformation($"Generated download URL for: {blobName}");

            return Ok(new
            {
                downloadUrl,
                expiresAt,
                fileName = metadata.FileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate download URL for: {blobName}");
            return StatusCode(500, new { error = "Failed to generate download URL" });
        }
    }

    /// <summary>
    /// Delete attachment (soft delete - moves to deleted-blobs container)
    /// </summary>
    [HttpDelete("{*blobName}")]
    public async Task<IActionResult> DeleteAttachment(string blobName)
    {
        try
        {
            // TODO: Validate user has permission to delete this attachment
            
            var success = await _blobStorageService.DeleteFileAsync(blobName);
            if (!success)
            {
                return NotFound(new { error = "Attachment not found or already deleted" });
            }

            _logger.LogInformation($"Deleted attachment: {blobName}");

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete attachment: {blobName}");
            return StatusCode(500, new { error = "Failed to delete attachment" });
        }
    }

    /// <summary>
    /// List all attachments for a vendor
    /// </summary>
    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> ListAttachments(string vendorId, [FromQuery] string? category = null)
    {
        try
        {
            // TODO: Validate user has permission to view this vendor's attachments
            
            var files = await _blobStorageService.ListFilesAsync("vendors", vendorId, category);

            return Ok(new
            {
                vendorId,
                category,
                count = files.Count,
                attachments = files.Select(f => new
                {
                    fileName = f.FileName,
                    blobName = f.FileId,
                    contentType = f.ContentType,
                    sizeBytes = f.FileSizeBytes,
                    uploadedAt = f.UploadedAt,
                    category = f.Category
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to list attachments for vendor: {vendorId}");
            return StatusCode(500, new { error = "Failed to list attachments" });
        }
    }
}

/// <summary>
/// Request model for requesting upload permission
/// </summary>
public class AttachmentUploadRequest
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required string Category { get; set; }
    public long SizeBytes { get; set; }
    public required string VendorId { get; set; }
}

/// <summary>
/// Request model for confirming successful upload
/// </summary>
public class AttachmentConfirmRequest
{
    public required string BlobName { get; set; }
    public required string VendorId { get; set; }
}
