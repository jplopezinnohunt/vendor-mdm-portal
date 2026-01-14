using VendorMdm.Shared.Models.FileStorage;

namespace VendorMdm.Api.Services;

/// <summary>
/// Interface for file storage service
/// Supports both Mock (local filesystem) and Real (Azure Blob Storage) implementations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file to storage
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(FileUploadRequest request);

    /// <summary>
    /// Download a file from storage
    /// </summary>
    Task<FileDownloadResult> DownloadFileAsync(string fileId);

    /// <summary>
    /// Get file metadata without downloading the file
    /// </summary>
    Task<FileMetadata> GetFileMetadataAsync(string fileId);

    /// <summary>
    /// List all files for a specific entity
    /// </summary>
    /// <param name="app">App name (e.g., "invitations")</param>
    /// <param name="entityId">Entity ID (e.g., "INV-2025-001")</param>
    /// <param name="process">Optional process filter (e.g., "documents")</param>
    Task<List<FileMetadata>> ListFilesAsync(string app, string entityId, string? process = null);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task<bool> DeleteFileAsync(string fileId);

    /// <summary>
    /// Generate a temporary download URL (SAS URL for Azure Blob)
    /// </summary>
    Task<string> GenerateDownloadUrlAsync(string fileId, int expirationMinutes = 60);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    Task<bool> FileExistsAsync(string fileId);

    /// <summary>
    /// Test connectivity to storage provider
    /// </summary>
    Task<bool> TestConnectionAsync();
}
