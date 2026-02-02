using System.Text;
using System.Security.Cryptography;

namespace VendorMdm.Api.Services;

/// <summary>
/// Service for field-level encryption of sensitive PII data.
/// Implements Pattern 17: Data Privacy & Masking (GDPR).
/// Uses AES-256 encryption with keys from Azure Key Vault.
/// </summary>
public interface IFieldEncryptionService
{
    Task<string> EncryptAsync(string plainText);
    Task<string> DecryptAsync(string cipherText);
}

public class FieldEncryptionService : IFieldEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FieldEncryptionService> _logger;
    private const int KeySize = 256;
    private const int BlockSize = 128;

    public FieldEncryptionService(
        IConfiguration configuration,
        ILogger<FieldEncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Encrypts plain text using AES-256.
    /// In production, encryption key comes from Azure Key Vault.
    /// </summary>
    public async Task<string> EncryptAsync(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            var key = await GetEncryptionKeyAsync();
            
            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            
            // Write IV first (needed for decryption)
            ms.Write(aes.IV, 0, aes.IV.Length);
            
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                await sw.WriteAsync(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw;
        }
    }

    /// <summary>
    /// Decrypts cipher text using AES-256.
    /// </summary>
    public async Task<string> DecryptAsync(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var key = await GetEncryptionKeyAsync();
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Key = key;

            // Extract IV from beginning of cipher text
            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(buffer, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return await sr.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw;
        }
    }

    /// <summary>
    /// Gets encryption key from Azure Key Vault (Production) or configuration (Development).
    /// </summary>
    private async Task<byte[]> GetEncryptionKeyAsync()
    {
        // In production, retrieve from Azure Key Vault
        var keyVaultUrl = _configuration["KeyVault:Url"];
        
        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            // TODO: Implement Key Vault retrieval
            // For now, use configuration-based key
            _logger.LogWarning("Using configuration-based encryption key. In production, use Azure Key Vault.");
        }

        // Development: Use configured key or generate deterministic key
        var configKey = _configuration["Encryption:Key"];
        if (!string.IsNullOrEmpty(configKey))
        {
            return Convert.FromBase64String(configKey);
        }

        // Fallback: Generate deterministic key from machine ID (NOT for production)
        _logger.LogWarning("Generating deterministic encryption key. This is NOT secure for production.");
        using var sha = SHA256.Create();
        var machineKey = Environment.MachineName + "VendorMDM-Encryption-Key";
        return sha.ComputeHash(Encoding.UTF8.GetBytes(machineKey));
    }
}
