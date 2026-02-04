using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models.FileStorage;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// File storage API for managing vendor documents
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/files")]
[Route("api/files")]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorage,
        ILogger<FilesController> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="app">Application context (e.g., "invitations", "vendors")</param>
    /// <param name="entityId">Entity ID (e.g., "INV-2025-001")</param>
    /// <param name="process">Process name (e.g., "documents", "kyc", "sanctions")</param>
    /// <param name="category">Document category (e.g., "passport", "sanctions-screening-report")</param>
    /// <param name="isConfidential">Whether file is confidential</param>
    /// <returns>Upload result with file ID</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)] // 10MB
    [ProducesResponseType(typeof(FileUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileUploadResult>> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string app,
        [FromForm] string entityId,
        [FromForm] string process,
        [FromForm] string category,
        [FromForm] bool isConfidential = false)
    {
        try
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
            {
                return BadRequest(new
                {
                    error = result.ErrorMessage,
                    validationErrors = result.ValidationErrors
                });
            }

            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get file metadata
    /// </summary>
    [HttpGet("{fileId}")]
    [ProducesResponseType(typeof(FileMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileMetadata>> GetFileMetadata(string fileId)
    {
        try
        {
            var metadata = await _fileStorage.GetFileMetadataAsync(fileId);
            return Ok(metadata);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = $"File {fileId} not found" });
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("download/{fileId}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        try
        {
            var result = await _fileStorage.DownloadFileAsync(fileId);

            if (!result.Success)
            {
                return NotFound(new { error = result.ErrorMessage });
            }

            return File(result.FileStream!, result.ContentType!, result.FileName);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// List files for an entity
    /// </summary>
    /// <param name="app">Application context</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="process">Optional process filter</param>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<FileMetadata>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FileMetadata>>> ListFiles(
        [FromQuery] string app,
        [FromQuery] string entityId,
        [FromQuery] string? process = null)
    {
        try
        {
            var files = await _fileStorage.ListFilesAsync(app, entityId, process);
            return Ok(files);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{fileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(string fileId)
    {
        try
        {
            var deleted = await _fileStorage.DeleteFileAsync(fileId);
            return deleted ? NoContent() : NotFound();
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Generate a temporary download URL
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <param name="expirationMinutes">URL expiration in minutes (default: 60)</param>
    [HttpGet("download-url/{fileId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDownloadUrl(
        string fileId,
        [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            var url = await _fileStorage.GenerateDownloadUrlAsync(fileId, expirationMinutes);
            return Ok(new { url, expiresInMinutes = expirationMinutes });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = $"File {fileId} not found" });
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Check if a file exists
    /// </summary>
    [HttpHead("{fileId}")]
    [HttpGet("exists/{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FileExists(string fileId)
    {
        try
        {
            var exists = await _fileStorage.FileExistsAsync(fileId);
            return exists ? Ok(new { exists = true }) : NotFound(new { exists = false });
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                error = "Real file storage service not configured",
                details = ex.Message
            });
        }
    }
}
