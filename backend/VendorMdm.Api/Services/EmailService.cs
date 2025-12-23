using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace VendorMdm.Api.Services;

/// <summary>
/// Email service implementation that supports multiple sending methods:
/// 1. Azure Function HTTP endpoint (for local dev when Function is running)
/// 2. SMTP (if configured)
/// 3. Logging (fallback for local dev)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly bool _useLocalEmulators;

    public EmailService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _useLocalEmulators = configuration.GetValue<bool>("UseLocalEmulators");
    }

    public async Task<bool> SendInvitationEmailAsync(InvitationEmailData data)
    {
        return await SendEmailAsync(data.Email, $"Onboarding Invitation for {data.VendorName}", data);
    }

    public async Task<bool> SendMfaCodeEmailAsync(string email, string vendorName, string code)
    {
        var data = new InvitationEmailData
        {
            Email = email,
            VendorName = vendorName,
            Notes = $"Your verification code is: {code}",
            // We use the same structure for now, but we'll build a specific HTML for MFA
        };

        return await SendEmailAsync(email, "Your Verification Code", data, isMfa: true, mfaCode: code);
    }

    private async Task<bool> SendEmailAsync(string email, string subject, InvitationEmailData data, bool isMfa = false, string? mfaCode = null)
    {
        try
        {
            // Prepare data for logging/functions
            var payload = new
            {
                Email = email,
                Subject = subject,
                VendorName = data.VendorName,
                IsMfa = isMfa,
                MfaCode = mfaCode,
                InvitationLink = data.BaseUrl != null ? $"{data.BaseUrl}/invitation/register/{data.Token}" : null,
                Notes = data.Notes
            };

            // Try Azure Function HTTP endpoint first (for local dev)
            if (_useLocalEmulators)
            {
                var functionUrl = _configuration["EmailService:FunctionUrl"] 
                    ?? "http://localhost:7071/api/invitation/send-email";
                
                // For simulation purposes, we'll just log if function call fails
                if (await TrySendViaFunctionAsync(functionUrl, data))
                {
                    return true;
                }
            }

            // Try SMTP if configured
            var smtpEnabled = _configuration.GetValue<bool>("EmailService:Smtp:Enabled", false);
            if (smtpEnabled)
            {
                if (await TrySendViaSmtpAsync(data, subject, isMfa, mfaCode))
                {
                    return true;
                }
            }

            // Fallback: Log email (for local development)
            LogEmail(email, subject, data, isMfa, mfaCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            LogEmail(email, subject, data, isMfa, mfaCode);
            return false;
        }
    }

    /// <summary>
    /// Try to send email via Azure Function HTTP endpoint
    /// </summary>
    private async Task<bool> TrySendViaFunctionAsync(string functionUrl, InvitationEmailData data)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var request = new
            {
                InvitationId = data.InvitationId,
                VendorName = data.VendorName,
                Email = data.Email,
                Token = data.Token,
                ExpiresAt = data.ExpiresAt.ToString("o"),
                InvitedByName = data.InvitedByName,
                CompanyName = data.CompanyName ?? "Your Company",
                Notes = data.Notes
            };

            var response = await httpClient.PostAsJsonAsync(functionUrl, request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Invitation email sent successfully via Function to {Email}",
                    data.Email);
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Function endpoint returned {StatusCode}, falling back to logging",
                    response.StatusCode);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(
                "Azure Function not available at {Url}: {Message}. Falling back to logging.",
                functionUrl,
                ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Try to send email via SMTP
    /// </summary>
    private async Task<bool> TrySendViaSmtpAsync(InvitationEmailData data, string subject, bool isMfa = false, string? mfaCode = null)
    {
        try
        {
            // Read from Key Vault (production) or local configuration (development)
            // Key Vault secrets use double underscore (__) instead of colon (:)
            var smtpHost = _configuration["EmailService:Smtp:Host"] 
                ?? _configuration["EmailService__Smtp__Host"];
            var smtpPort = _configuration.GetValue<int>("EmailService:Smtp:Port", 587);
            if (smtpPort == 587)
            {
                smtpPort = _configuration.GetValue<int>("EmailService__Smtp__Port", 587);
            }
            
            var smtpUsername = _configuration["EmailService:Smtp:Username"] 
                ?? _configuration["EmailService__Smtp__Username"];
            var smtpPassword = _configuration["EmailService:Smtp:Password"] 
                ?? _configuration["EmailService__Smtp__Password"];
            var fromEmail = _configuration["EmailService:Smtp:FromEmail"] 
                ?? _configuration["EmailService__Smtp__FromEmail"] 
                ?? smtpUsername;
            var fromName = _configuration["EmailService:Smtp:FromName"] 
                ?? _configuration["EmailService__Smtp__FromName"] 
                ?? "Vendor Management";
            var useSsl = _configuration.GetValue<bool>("EmailService:Smtp:UseSsl", true);

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("SMTP configuration incomplete. Missing Host, Username, or Password.");
                return false;
            }

            var baseUrl = data.BaseUrl ?? _configuration["App:BaseUrl"] ?? "http://localhost:3000";
            var invitationLink = $"{baseUrl}/invitation/register/{data.Token}";
            var expiresAt = data.ExpiresAt.ToString("MMMM dd, yyyy 'at' hh:mm tt");
            var companyName = data.CompanyName ?? "Your Company";

            // Create email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(data.VendorName, data.Email));
            message.Subject = subject;

            // Build HTML email body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = BuildEmailHtml(data, invitationLink, expiresAt, companyName, isMfa, mfaCode)
            };

            message.Body = bodyBuilder.ToMessageBody();

            // Send email via SMTP
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation("✅ Email sent successfully via SMTP to {Email}", data.Email);
            Console.WriteLine($"✅ Email sent via SMTP to: {data.Email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {Email}", data.Email);
            Console.WriteLine($"❌ SMTP error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Build HTML email content
    /// </summary>
    private string BuildEmailHtml(InvitationEmailData data, string invitationLink, string expiresAt, string companyName, bool isMfa = false, string? mfaCode = null)
    {
        if (isMfa)
        {
            return $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee;'>
                    <h2 style='color: #0078d4;'>Verification Code</h2>
                    <p>Hello {data.VendorName},</p>
                    <p>To access the UNESCO Vendor Onboarding portal, please use the following verification code:</p>
                    <div style='background: #f4f4f4; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                        {mfaCode}
                    </div>
                    <p>This code will expire in 15 minutes.</p>
                    <p>If you did not request this code, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #666;'>This is an automated message from the UNESCO Vendor Portal.</p>
                </div>";
        }
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Vendor Invitation</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f5f5f5; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">Vendor Invitation</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;"">
                                Dear {data.VendorName} Team,
                            </p>
                            
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;"">
                                You have been invited by <strong>{data.InvitedByName}</strong> to register as an approved vendor with <strong>{companyName}</strong>.
                            </p>
                            
                            <p style=""color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;"">
                                To complete your registration, please click the button below. This process should take approximately <strong>15-20 minutes</strong>.
                            </p>
                            
                            <!-- CTA Button -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 20px 0;"">
                                        <a href=""{invitationLink}"" style=""display: inline-block; padding: 16px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-size: 18px; font-weight: 600; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                                            Start Registration Now
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Expiration Notice -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #fff3cd; border-left: 4px solid #ffc107; margin: 30px 0;"">
                                <tr>
                                    <td style=""padding: 15px;"">
                                        <p style=""color: #856404; margin: 0; font-size: 14px;"">
                                            ⏰ <strong>Important:</strong> This invitation link will expire on <strong>{expiresAt}</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Alternative Link -->
                            <p style=""color: #666666; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                If the button doesn't work, copy and paste this link into your browser:
                            </p>
                            <p style=""color: #667eea; font-size: 13px; word-break: break-all; margin: 10px 0 0 0;"">
                                {invitationLink}
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 20px; text-align: center; background-color: #333333; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                © {DateTime.UtcNow.Year} {companyName}. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    /// <summary>
    /// Log email details (fallback for local development)
    /// </summary>
    private void LogEmail(string email, string subject, InvitationEmailData data, bool isMfa = false, string? mfaCode = null)
    {
        if (isMfa)
        {
            Console.WriteLine("");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("📧 MFA VERIFICATION CODE (LOCAL DEV)");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Code: {mfaCode}");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("");
            return;
        }

        var baseUrl = data.BaseUrl ?? _configuration["App:BaseUrl"] ?? "http://localhost:3002";
        var invitationLink = $"{baseUrl}/invitation/register/{data.Token}";
        var expiresAt = data.ExpiresAt.ToString("MMMM dd, yyyy 'at' hh:mm tt");
        var companyName = data.CompanyName ?? "Your Company";

        // Use Console.WriteLine for better visibility in development
        Console.WriteLine("");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("📧 INVITATION EMAIL (LOCAL DEV - EMAIL SENT)");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine($"To: {data.Email}");
        Console.WriteLine($"Subject: Action Required: Invitation to Register as Vendor with {companyName}");
        Console.WriteLine($"Vendor Name: {data.VendorName}");
        Console.WriteLine($"Invited By: {data.InvitedByName}");
        Console.WriteLine($"Invitation Link: {invitationLink}");
        Console.WriteLine($"Expires: {expiresAt}");
        if (!string.IsNullOrEmpty(data.Notes))
        {
            Console.WriteLine($"Notes: {data.Notes}");
        }
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("");

        _logger.LogInformation("===== INVITATION EMAIL (LOCAL DEV) =====");
        _logger.LogInformation("To: {Email}", data.Email);
        _logger.LogInformation("Subject: Action Required: Invitation to Register as Vendor with {CompanyName}", companyName);
        _logger.LogInformation("Vendor Name: {VendorName}", data.VendorName);
        _logger.LogInformation("Invited By: {InvitedByName}", data.InvitedByName);
        _logger.LogInformation("Invitation Link: {Link}", invitationLink);
        _logger.LogInformation("Expires: {ExpiresAt}", expiresAt);
        if (!string.IsNullOrEmpty(data.Notes))
        {
            _logger.LogInformation("Notes: {Notes}", data.Notes);
        }
        _logger.LogInformation("========================================");
        _logger.LogInformation("");
        _logger.LogInformation("📧 EMAIL CONTENT:");
        _logger.LogInformation("");
        _logger.LogInformation("Dear {VendorName} Team,", data.VendorName);
        _logger.LogInformation("");
        _logger.LogInformation("You have been invited by {InvitedByName} to register as an approved vendor with {CompanyName}.", 
            data.InvitedByName, companyName);
        _logger.LogInformation("");
        _logger.LogInformation("To complete your registration, please click the link below:");
        _logger.LogInformation("{Link}", invitationLink);
        _logger.LogInformation("");
        _logger.LogInformation("⏰ Important: This invitation link will expire on {ExpiresAt}", expiresAt);
        _logger.LogInformation("");
        _logger.LogInformation("========================================");
    }
}

