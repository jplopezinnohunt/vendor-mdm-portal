# File Storage Standard

**Category**: Integration & Infrastructure
**Pattern #**: 14
**Status**: MANDATORY

---

## Definition

File storage MUST use `IFileStorageService` interface with configuration-driven provider selection.

---

## Rules

1. **ALWAYS** use `IFileStorageService` interface
2. **ALWAYS** implement simulation mode for local dev
3. **NEVER** store files in database BLOB
4. **ALWAYS** validate file types and sizes

---

## Implementation

### Interface

```csharp
public interface IFileStorageService
{
    Task<Result<string>> UploadAsync(Stream content, string fileName, string folder);
    Task<Result<Stream>> DownloadAsync(string filePath);
    Task<Result> DeleteAsync(string filePath);
    Task<bool> ExistsAsync(string filePath);
}
```

### Azure Blob Implementation

```csharp
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobClient;

    public async Task<Result<string>> UploadAsync(Stream content, string fileName, string folder)
    {
        var containerClient = _blobClient.GetBlobContainerClient(folder);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(content, overwrite: true);

        return Result<string>.Success(blobClient.Uri.ToString());
    }
}
```

### Simulation Implementation

```csharp
public class SimulatedFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly IStructuredLogger _logger;

    public async Task<Result<string>> UploadAsync(Stream content, string fileName, string folder)
    {
        var filePath = Path.Combine(_basePath, folder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);

        _logger.LogInformation("[SIMULATION MODE] File uploaded", new {
            filePath,
            size = content.Length
        });

        return Result<string>.Success(filePath);
    }
}
```

### Registration (Program.cs)

```csharp
if (builder.Configuration.GetValue<bool>("FileStorage:UseMock"))
{
    builder.Services.AddSingleton<IFileStorageService, SimulatedFileStorageService>();
}
else
{
    builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
}
```

### File Validation

```csharp
public static class FileValidation
{
    public static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".png", ".jpg" };
    public const int MaxFileSizeMb = 10;

    public static Result ValidateFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return Result.Failure($"File type {extension} not allowed");

        if (file.Length > MaxFileSizeMb * 1024 * 1024)
            return Result.Failure($"File exceeds {MaxFileSizeMb}MB limit");

        return Result.Success();
    }
}
```

---

## Reference

- **Interface**: `Core.Framework/Storage/IFileStorageService.cs`
- **Golden Rules**: Section 10.4 Pattern 14
