using VendorMdm.Core.Framework.Primitives;

namespace VendorMdm.Core.Framework.FileSystem;

/// <summary>
/// Core file storage service for all MDM applications.
/// Provides abstraction over Azure Blob Storage with support for local development.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="fileStream">File stream to upload</param>
    /// <param name="fileName">File name</param>
    /// <param name="containerName">Container/folder name</param>
    /// <param name="metadata">Optional metadata</param>
    /// <returns>Blob path if successful</returns>
    Task<Result<string>> UploadAsync(
        Stream fileStream, 
        string fileName, 
        string containerName,
        Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="blobPath">Full blob path</param>
    /// <returns>File stream if successful</returns>
    Task<Result<Stream>> DownloadAsync(string blobPath);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="blobPath">Full blob path</param>
    /// <returns>Success or failure</returns>
    Task<Result> DeleteAsync(string blobPath);

    /// <summary>
    /// Lists all files in a container with optional prefix.
    /// </summary>
    /// <param name="containerName">Container/folder name</param>
    /// <param name="prefix">Optional prefix filter</param>
    /// <returns>List of blob paths</returns>
    Task<Result<IEnumerable<string>>> ListAsync(string containerName, string prefix = "");

    /// <summary>
    /// Gets file metadata.
    /// </summary>
    /// <param name="blobPath">Full blob path</param>
    /// <returns>File metadata if successful</returns>
    Task<Result<FileMetadata>> GetMetadataAsync(string blobPath);

    /// <summary>
    /// Generates a temporary download URL (SAS token).
    /// </summary>
    /// <param name="blobPath">Full blob path</param>
    /// <param name="expiresIn">Expiration duration</param>
    /// <returns>Temporary URL if successful</returns>
    Task<Result<string>> GenerateDownloadUrlAsync(string blobPath, TimeSpan expiresIn);
}

/// <summary>
/// File metadata.
/// </summary>
public record FileMetadata
{
    public required string BlobPath { get; init; }
    public required string FileName { get; init; }
    public required long SizeBytes { get; init; }
    public required string ContentType { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime LastModifiedAt { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}
