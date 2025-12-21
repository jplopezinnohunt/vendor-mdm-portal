namespace VendorMdm.Shared.Models.FileStorage;

/// <summary>
/// Request model for file upload
/// </summary>
public class FileUploadRequest
{
    /// <summary>
    /// Application context (e.g., "invitations", "vendors", "workflows")
    /// </summary>
    public string App { get; set; } = null!;

    /// <summary>
    /// Entity ID within the app (e.g., "INV-2025-001", "VEN-12345")
    /// </summary>
    public string EntityId { get; set; } = null!;

    /// <summary>
    /// Process within the app (e.g., "documents", "kyc", "bank")
    /// </summary>
    public string Process { get; set; } = null!;

    /// <summary>
    /// Document category (e.g., "passport", "bank-certificate")
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// File stream
    /// </summary>
    public Stream FileStream { get; set; } = null!;

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// MIME content type
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Whether this file contains confidential information
    /// </summary>
    public bool IsConfidential { get; set; }

    /// <summary>
    /// User who uploaded the file
    /// </summary>
    public string UploadedBy { get; set; } = null!;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Result of file upload operation
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FileId { get; set; }
    public string? StoragePath { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

/// <summary>
/// File metadata
/// </summary>
public class FileMetadata
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
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Result of file download operation
/// </summary>
public class FileDownloadResult
{
    public bool Success { get; set; }
    public Stream? FileStream { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
}
