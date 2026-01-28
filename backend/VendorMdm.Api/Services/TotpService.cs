using System.Security.Cryptography;
using System.Text;

namespace VendorMdm.Api.Services;

public class TotpService : ITotpService
{
    // Configuration
    private const int Step = 30;
    private const int Size = 6;
    private readonly string _issuer;

    public TotpService(IConfiguration configuration)
    {
        _issuer = configuration["App:CompanyName"] ?? "VendorMDM";
    }

    public string GenerateSecret()
    {
        // Generate a random 20-byte key (160 bits)
        var buffer = new byte[20];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        
        // Return as Base32 string (simplified implementation or use a library if available)
        // For simplicity in this "Copy/Paste" environment, we'll use a simple Base32 converter
        return Base32Encoding.ToString(buffer);
    }

    public string GenerateQrCodeUri(string email, string secret)
    {
        // format: otpauth://totp/Issuer:Email?secret=SECRET&issuer=Issuer
        return $"otpauth://totp/{Uri.EscapeDataString(_issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(_issuer)}";
    }

    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code)) return false;

        try 
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            // Verify current window and previous/next window (skew)
            long currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / Step;
            
            for (int i = -1; i <= 1; i++)
            {
                if (ValidateHotp(secretBytes, currentStep + i, code)) return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateHotp(byte[] secret, long counter, string code)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(counterBytes);

        int offset = hash[hash.Length - 1] & 0xf;
        int binary = ((hash[offset] & 0x7f) << 24)
                   | ((hash[offset + 1] & 0xff) << 16)
                   | ((hash[offset + 2] & 0xff) << 8)
                   | (hash[offset + 3] & 0xff);

        int otp = binary % 1000000;
        return otp.ToString("D6") == code;
    }
}

// Simple Base32 Helper to avoid external dependencies for now
// Standard RFC 4648
public static class Base32Encoding
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string ToString(byte[] input)
    {
        if (input == null || input.Length == 0) return string.Empty;

        var output = new StringBuilder();
        int bitData = 0;
        int bitCount = 0;

        foreach (var b in input)
        {
            bitData = (bitData << 8) | b;
            bitCount += 8;

            while (bitCount >= 5)
            {
                int index = (bitData >> (bitCount - 5)) & 0x1f;
                output.Append(Alphabet[index]);
                bitCount -= 5;
            }
        }

        if (bitCount > 0)
        {
            int index = (bitData << (5 - bitCount)) & 0x1f;
            output.Append(Alphabet[index]);
        }

        return output.ToString();
    }

    public static byte[] ToBytes(string input)
    {
        if (string.IsNullOrEmpty(input)) return Array.Empty<byte>();

        input = input.Trim().ToUpper().Replace(" ", "").Replace("-", "");
        var output = new List<byte>();
        int bitData = 0;
        int bitCount = 0;

        foreach (var c in input)
        {
            int index = Alphabet.IndexOf(c);
            if (index == -1) continue; // Skip invalid chars

            bitData = (bitData << 5) | index;
            bitCount += 5;

            if (bitCount >= 8)
            {
                output.Add((byte)((bitData >> (bitCount - 8)) & 0xff));
                bitCount -= 8;
            }
        }
        return output.ToArray();
    }
}
