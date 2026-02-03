using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Core.Framework.Logging;

namespace VendorMdm.Core.Framework.FileSystem;

/// <summary>
/// Azure Blob Storage implementation of IFileStorageService.
/// </summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IStructuredLogger _logger;
    private readonly AzureBlobStorageOptions _options;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        IStructuredLogger logger,
        AzureBlobStorageOptions options)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<Result<string>> UploadAsync(
        Stream fileStream,
        string fileName,
        string containerName,
        Dictionary<string, string>? metadata = null)
    {
        using var _ = _logger.BeginOperation("UploadFile", ("FileName", fileName), ("Container", containerName));

        try
        {
            // Validate inputs
            if (fileStream == null || fileStream.Length == 0)
                return Result.Fail<string>("File stream is empty");

            if (string.IsNullOrWhiteSpace(fileName))
                return Result.Fail<string>("File name is required");

            if (string.IsNullOrWhiteSpace(containerName))
                return Result.Fail<string>("Container name is required");

            // Get or create container
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Generate unique blob name
            var blobName = $"{Guid.NewGuid()}/{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload file
            var uploadOptions = new BlobUploadOptions
            {
                Metadata = metadata
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);

            var blobPath = $"{containerName}/{blobName}";
            _logger.LogInformation("File uploaded successfully", ("BlobPath", blobPath), ("Size", fileStream.Length));

            return Result.Ok(blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File upload failed", ("FileName", fileName));
            return Result.Fail<string>($"File upload failed: {ex.Message}");
        }
    }

    public async Task<Result<Stream>> DownloadAsync(string blobPath)
    {
        using var _ = _logger.BeginOperation("DownloadFile", ("BlobPath", blobPath));

        try
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                return Result.Fail<Stream>("Blob path is required");

            // Parse blob path (format: container/folder/filename)
            var parts = blobPath.Split('/', 2);
            if (parts.Length != 2)
                return Result.Fail<Stream>("Invalid blob path format");

            var containerName = parts[0];
            var blobName = parts[1];

            // Get blob client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found", ("BlobPath", blobPath));
                return Result.Fail<Stream>("File not found");
            }

            // Download blob
            var response = await blobClient.DownloadAsync();
            var stream = new MemoryStream();
            await response.Value.Content.CopyToAsync(stream);
            stream.Position = 0;

            _logger.LogInformation("File downloaded successfully", ("BlobPath", blobPath), ("Size", stream.Length));
            return Result.Ok<Stream>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File download failed", ("BlobPath", blobPath));
            return Result.Fail<Stream>($"File download failed: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(string blobPath)
    {
        using var _ = _logger.BeginOperation("DeleteFile", ("BlobPath", blobPath));

        try
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                return Result.Fail("Blob path is required");

            // Parse blob path
            var parts = blobPath.Split('/', 2);
            if (parts.Length != 2)
                return Result.Fail("Invalid blob path format");

            var containerName = parts[0];
            var blobName = parts[1];

            // Get blob client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Delete blob
            await blobClient.DeleteIfExistsAsync();

            _logger.LogInformation("File deleted successfully", ("BlobPath", blobPath));
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File deletion failed", ("BlobPath", blobPath));
            return Result.Fail($"File deletion failed: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<string>>> ListAsync(string containerName, string prefix = "")
    {
        using var _ = _logger.BeginOperation("ListFiles", ("Container", containerName), ("Prefix", prefix));

        try
        {
            if (string.IsNullOrWhiteSpace(containerName))
                return Result.Fail<IEnumerable<string>>("Container name is required");

            // Get container client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Check if container exists
            if (!await containerClient.ExistsAsync())
            {
                _logger.LogWarning("Container not found", ("Container", containerName));
                return Result.Ok<IEnumerable<string>>(Enumerable.Empty<string>());
            }

            // List blobs
            var blobPaths = new List<string>();
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobPaths.Add($"{containerName}/{blobItem.Name}");
            }

            _logger.LogInformation("Files listed successfully", ("Container", containerName), ("Count", blobPaths.Count));
            return Result.Ok<IEnumerable<string>>(blobPaths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File listing failed", ("Container", containerName));
            return Result.Fail<IEnumerable<string>>($"File listing failed: {ex.Message}");
        }
    }

    public async Task<Result<FileMetadata>> GetMetadataAsync(string blobPath)
    {
        using var _ = _logger.BeginOperation("GetMetadata", ("BlobPath", blobPath));

        try
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                return Result.Fail<FileMetadata>("Blob path is required");

            // Parse blob path
            var parts = blobPath.Split('/', 2);
            if (parts.Length != 2)
                return Result.Fail<FileMetadata>("Invalid blob path format");

            var containerName = parts[0];
            var blobName = parts[1];

            // Get blob client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found", ("BlobPath", blobPath));
                return Result.Fail<FileMetadata>("File not found");
            }

            // Get properties
            var properties = await blobClient.GetPropertiesAsync();

            var metadata = new FileMetadata
            {
                BlobPath = blobPath,
                FileName = Path.GetFileName(blobName),
                SizeBytes = properties.Value.ContentLength,
                ContentType = properties.Value.ContentType,
                CreatedAt = properties.Value.CreatedOn.DateTime,
                LastModifiedAt = properties.Value.LastModified.DateTime,
                Metadata = properties.Value.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            _logger.LogInformation("Metadata retrieved successfully", ("BlobPath", blobPath));
            return Result.Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metadata retrieval failed", ("BlobPath", blobPath));
            return Result.Fail<FileMetadata>($"Metadata retrieval failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> GenerateDownloadUrlAsync(string blobPath, TimeSpan expiresIn)
    {
        using var _ = _logger.BeginOperation("GenerateDownloadUrl", ("BlobPath", blobPath), ("ExpiresIn", expiresIn.TotalMinutes));

        try
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                return Result.Fail<string>("Blob path is required");

            // Parse blob path
            var parts = blobPath.Split('/', 2);
            if (parts.Length != 2)
                return Result.Fail<string>("Invalid blob path format");

            var containerName = parts[0];
            var blobName = parts[1];

            // Get blob client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found", ("BlobPath", blobPath));
                return Result.Fail<string>("File not found");
            }

            // Generate SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobClient.GenerateSasUri(sasBuilder);
            var downloadUrl = sasToken.ToString();

            _logger.LogInformation("Download URL generated successfully", ("BlobPath", blobPath), ("ExpiresAt", sasBuilder.ExpiresOn));
            return Result.Ok(downloadUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download URL generation failed", ("BlobPath", blobPath));
            return Result.Fail<string>($"Download URL generation failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Configuration options for Azure Blob Storage.
/// </summary>
public class AzureBlobStorageOptions
{
    public required string ConnectionString { get; set; }
    public string? DefaultContainer { get; set; }
}
