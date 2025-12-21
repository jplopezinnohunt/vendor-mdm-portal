using VendorMdm.Shared.Models.FileStorage;

namespace VendorMdm.Api.Services;

/// <summary>
/// REAL implementation of file storage service using Azure Blob Storage
/// This is a skeleton - will be completed when Azure Blob Storage is configured
/// </summary>
public class FileStorageAzureBlobService : IFileStorageService
{
    private readonly ILogger<FileStorageAzureBlobService> _logger;
    private readonly IConfiguration _configuration;

    public FileStorageAzureBlobService(
        ILogger<FileStorageAzureBlobService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        _logger.LogInformation("AZURE BLOB: File Storage Service initialized (requires Azure Blob Storage configuration)");
    }

    public Task<FileUploadResult> UploadFileAsync(FileUploadRequest request)
    {
        _logger.LogWarning("AZURE BLOB: Not yet implemented - requires Azure Blob Storage setup");
        throw new NotImplementedException(
            "Azure Blob Storage is not configured yet. " +
            "Please configure Azure Storage connection string and container name, " +
            "or use Mock service by setting Services:FileStorage:UseMock=true");
    }

    public Task<FileDownloadResult> DownloadFileAsync(string fileId)
    {
        throw new NotImplementedException("Azure Blob Storage not configured");
    }

    public Task<FileMetadata> GetFileMetadataAsync(string fileId)
    {
        throw new NotImplementedException("Azure Blob Storage not configured");
    }

    public Task<List<FileMetadata>> ListFilesAsync(string app, string entityId, string? process = null)
    {
        throw new NotImplementedException("Azure Blob Storage not configured");
    }

    public Task<bool> DeleteFileAsync(string fileId)
    {
        throw new NotImplementedException("Azure Blob Storage not configured");
    }

    public Task<string> GenerateDownloadUrlAsync(string fileId, int expirationMinutes = 60)
    {
        throw new NotImplementedException("Azure Blob Storage not configured");
    }

    public Task<bool> FileExistsAsync(string fileId)
    {
        throw new NotImplementedException("Azure Blob Storage not configured");
    }

    /* 
     * TODO: Implement with Azure.Storage.Blobs
     * 
     * Required NuGet packages:
     * - Azure.Storage.Blobs
     * 
     * Configuration needed in appsettings.json:
     * "Azure": {
     *   "Storage": {
     *     "ConnectionString": "from Key Vault",
     *     "ContainerName": "vendor-documents"
     *   }
     * }
     * 
     * Service registration in Program.cs:
     * builder.Services.AddSingleton(sp => {
     *     var connectionString = builder.Configuration["Azure:Storage:ConnectionString"];
     *     return new BlobServiceClient(connectionString);
     * });
     * 
     * Implementation pattern:
     * 1. Get BlobServiceClient via DI
     * 2. Get container: _blobClient.GetBlobContainerClient(containerName)
     * 3. Upload: blobClient.UploadAsync(stream, options)
     * 4. Download: blobClient.DownloadAsync()
     * 5. Generate SAS URL: blobClient.GenerateSasUri(sasBuilder)
     * 6. Store metadata in SQL database
     */
}
