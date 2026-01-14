using VendorMdm.Shared.Models.FileStorage;

namespace VendorMdm.Api.Services;

/// <summary>
/// MOCK implementation of file storage service using local filesystem
/// Used for local development AND production deployment until Azure Blob is connected
/// </summary>
public class FileStorageSimulationService : IFileStorageService
{
    private readonly ILogger<FileStorageSimulationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _basePath;
    private readonly Dictionary<string, FileMetadata> _mockMetadata;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedExtensions;

    public FileStorageSimulationService(
        ILogger<FileStorageSimulationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Get configuration
        _basePath = configuration["Services:FileStorage:MockSettings:TempPath"] 
            ?? Path.Combine(Path.GetTempPath(), "vendor-mdm-files");
        _maxFileSizeBytes = configuration.GetValue<long>("Services:FileStorage:MaxFileSizeBytes", 10485760); // 10MB default
        _allowedExtensions = configuration.GetSection("Services:FileStorage:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        
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
            var extension = Path.GetExtension(request.FileName);
            var storagePath = BuildStoragePath(
                request.App, request.EntityId, request.Process,
                request.Category, fileId, extension);

            var fullPath = Path.Combine(_basePath, storagePath);
            var directory = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directory);

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
                FileSizeBytes = request.FileSizeBytes,
                IsConfidential = request.IsConfidential,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = request.UploadedBy,
                VirusScanStatus = "Clean", // Mock always returns clean
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            _mockMetadata[fileId] = metadata;

            result.Success = true;
            result.FileId = fileId;
            result.StoragePath = storagePath;

            _logger.LogInformation("MOCK: File uploaded successfully: {FileId} → {Path}", fileId, storagePath);
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
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("MOCK: File metadata exists but file not found on disk: {Path}", fullPath);
                return new FileDownloadResult
                {
                    Success = false,
                    ErrorMessage = "File not found on disk"
                };
            }

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

        _logger.LogInformation("MOCK: Listed {Count} files for {App}/{EntityId}/{Process}", 
            files.Count, app, entityId, process ?? "all");

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
        // Mock returns a simple relative URL
        var url = $"/api/files/download/{fileId}?mock=true&expires={expirationMinutes}";
        return Task.FromResult(url);
    }

    public Task<bool> FileExistsAsync(string fileId)
    {
        return Task.FromResult(_mockMetadata.ContainsKey(fileId));
    }

    public Task<bool> TestConnectionAsync()
    {
        _logger.LogInformation("MOCK FILE STORAGE: Testing connection (checking base path)");
        return Task.FromResult(Directory.Exists(_basePath));
    }

    // Helper methods

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

        // Max file size
        if (request.FileSizeBytes > _maxFileSizeBytes)
        {
            errors.Add($"File size ({request.FileSizeBytes / 1048576}MB) exceeds maximum of {_maxFileSizeBytes / 1048576}MB");
        }

        // Allowed extensions
        var extension = Path.GetExtension(request.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
        {
            errors.Add($"File type {extension} not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
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
        if (string.IsNullOrWhiteSpace(request.FileName))
            errors.Add("FileName is required");
        if (request.FileStream == null)
            errors.Add("FileStream is required");

        return errors;
    }
}
