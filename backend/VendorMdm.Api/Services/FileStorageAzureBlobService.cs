using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using VendorMdm.Shared.Models.FileStorage;

namespace VendorMdm.Api.Services;

/// <summary>
/// REAL implementation of Azure Blob Storage service using Azure.Storage.Blobs SDK
/// Implements Gold Standard Direct Upload pattern with SAS tokens
/// </summary>
public class FileStorageAzureBlobService : IFileStorageService
{
    private readonly ILogger<FileStorageAzureBlobService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "vendor-attachments";
    private readonly string _deletedContainerName = "deleted-blobs";

    public FileStorageAzureBlobService(
        ILogger<FileStorageAzureBlobService> logger,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
        
        _logger.LogInformation("Azure Blob Storage Service initialized");
    }

    public async Task<FileUploadResult> UploadFileAsync(FileUploadRequest request)
    {
        try
        {
            // Validate file type
            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            
            if (!allowedTypes.Contains(request.ContentType.ToLower()))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "File type not allowed",
                    ValidationErrors = new List<string> { $"Allowed types: {string.Join(", ", allowedTypes)}" }
                };
            }

            // Validate file size (10MB max)
            const long maxSize = 10 * 1024 * 1024;
            if (request.FileSizeBytes > maxSize)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File size exceeds {maxSize / 1024 / 1024}MB limit"
                };
            }

            // Generate unique blob name
            var extension = Path.GetExtension(request.FileName);
            var blobName = $"{request.App}/{request.EntityId}/{request.Process}/{Guid.NewGuid()}{extension}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload blob with metadata
            var metadata = new Dictionary<string, string>
            {
                { "OriginalFileName", request.FileName },
                { "UploadedBy", request.UploadedBy },
                { "Category", request.Category },
                { "IsConfidential", request.IsConfidential.ToString() }
            };

            await blobClient.UploadAsync(request.FileStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = request.ContentType
                },
                Metadata = metadata
            });

            _logger.LogInformation($"Blob uploaded successfully: {blobName}");

            return new FileUploadResult
            {
                Success = true,
                FileId = blobName,
                StoragePath = blobClient.Uri.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload blob");
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> GenerateUploadSasTokenAsync(string blobName, int expirationMinutes = 5)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    public async Task<FileDownloadResult> DownloadFileAsync(string fileId)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileId);

            var download = await blobClient.DownloadAsync();
            var properties = await blobClient.GetPropertiesAsync();

            return new FileDownloadResult
            {
                Success = true,
                FileStream = download.Value.Content,
                FileName = properties.Value.Metadata.TryGetValue("OriginalFileName", out var name) ? name : fileId,
                ContentType = properties.Value.ContentType,
                FileSizeBytes = properties.Value.ContentLength
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to download blob: {fileId}");
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> GenerateDownloadUrlAsync(string fileId, int expirationMinutes = 15)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileId);

        // Determine Content-Disposition based on file type
        var properties = await blobClient.GetPropertiesAsync();
        var contentType = properties.Value.ContentType;
        var disposition = contentType.StartsWith("image/") || contentType == "application/pdf" 
            ? "inline" 
            : "attachment";

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = fileId,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
            ContentDisposition = disposition
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string fileId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileId);
        var properties = await blobClient.GetPropertiesAsync();

        return new FileMetadata
        {
            FileId = fileId,
            FileName = properties.Value.Metadata.TryGetValue("OriginalFileName", out var name) ? name : fileId,
            StoragePath = blobClient.Uri.ToString(),
            ContentType = properties.Value.ContentType,
            FileSizeBytes = properties.Value.ContentLength,
            UploadedAt = properties.Value.CreatedOn.DateTime,
            UploadedBy = properties.Value.Metadata.TryGetValue("UploadedBy", out var user) ? user : "Unknown",
            Category = properties.Value.Metadata.TryGetValue("Category", out var cat) ? cat : "",
            IsConfidential = properties.Value.Metadata.TryGetValue("IsConfidential", out var conf) && conf == "True"
        };
    }

    public async Task<List<FileMetadata>> ListFilesAsync(string app, string entityId, string? process = null)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var prefix = process != null ? $"{app}/{entityId}/{process}/" : $"{app}/{entityId}/";
        
        var files = new List<FileMetadata>();
        await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix))
        {
            files.Add(new FileMetadata
            {
                FileId = blob.Name,
                FileName = blob.Metadata.TryGetValue("OriginalFileName", out var name) ? name : blob.Name,
                StoragePath = containerClient.GetBlobClient(blob.Name).Uri.ToString(),
                ContentType = blob.Properties.ContentType ?? "application/octet-stream",
                FileSizeBytes = blob.Properties.ContentLength ?? 0,
                UploadedAt = blob.Properties.CreatedOn?.DateTime ?? DateTime.MinValue,
                Category = blob.Metadata.TryGetValue("Category", out var cat) ? cat : ""
            });
        }

        return files;
    }

    public async Task<bool> DeleteFileAsync(string fileId)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileId);

            // Move to deleted container instead of hard delete
            var deletedContainer = _blobServiceClient.GetBlobContainerClient(_deletedContainerName);
            await deletedContainer.CreateIfNotExistsAsync();

            var deletedBlobClient = deletedContainer.GetBlobClient(fileId);
            await deletedBlobClient.StartCopyFromUriAsync(blobClient.Uri);
            
            // Delete from original container (soft delete will retain for 30 days)
            await blobClient.DeleteIfExistsAsync();

            _logger.LogInformation($"Blob moved to deleted container: {fileId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete blob: {fileId}");
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileId);
        return await blobClient.ExistsAsync();
    }
}
