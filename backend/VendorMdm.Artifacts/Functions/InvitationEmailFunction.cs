using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker.Http;

namespace VendorMdm.Artifacts.Functions;

/// <summary>
/// Azure Function to send vendor invitation emails
/// Triggered by Service Bus queue or HTTP endpoint
/// </summary>
public class InvitationEmailFunction
{
    private readonly ILogger _logger;
    // TODO: Inject Azure Communication Services Email Client or SendGrid
    // private readonly EmailClient _emailClient;

    public InvitationEmailFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<InvitationEmailFunction>();
    }

    /// <summary>
    /// Service Bus triggered function to send invitation email
    /// Message format: { "invitationId", "vendorName", "email", "token", "expiresAt", "invitedByName" }
    /// </summary>
    [Function("SendInvitationEmail")]
    public async Task SendInvitationEmailFromQueue(
        [ServiceBusTrigger("invitation-emails", Connection = "ServiceBusConnection")] string message)
    {
        _logger.LogInformation("Processing invitation email from Service Bus: {Message}", message);

        try
        {
            var invitationData = JsonConvert.DeserializeObject<InvitationEmailRequest>(message);
            
            if (invitationData == null)
            {
                _logger.LogError("Invalid invitation email message format");
                return;
            }

            await SendInvitationEmailAsync(invitationData);
            
            _logger.LogInformation(
                "Invitation email sent successfully to {Email} for invitation {InvitationId}", 
                invitationData.Email, 
                invitationData.InvitationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation email");
            throw; // Let Service Bus handle retry
        }
    }

    /// <summary>
    /// HTTP triggered function for manual/testing email sending
    /// </summary>
    [Function("SendInvitationEmailHttp")]
    public async Task<HttpResponseData> SendInvitationEmailHttp(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "invitation/send-email")] 
        HttpRequestData req)
    {
        _logger.LogInformation("Processing manual invitation email request");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var invitationData = JsonConvert.DeserializeObject<InvitationEmailRequest>(requestBody);

            if (invitationData == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid request payload");
                return badResponse;
            }

            await SendInvitationEmailAsync(invitationData);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { 
                Success = true, 
                Message = $"Invitation email sent to {invitationData.Email}" 
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation email");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Core email sending logic
    /// </summary>
    private async Task SendInvitationEmailAsync(InvitationEmailRequest data)
    {
        _logger.LogInformation("Sending invitation email to {Email}", data.Email);

        // Generate invitation link
        var baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "https://vendor-portal.company.com";
        var invitationLink = $"{baseUrl}/invitation/register/{data.Token}";

        // Build email content
        var emailSubject = $"Action Required: Invitation to Register as Vendor with {data.CompanyName ?? "Our Company"}";
        var emailBody = BuildEmailHtml(data, invitationLink);

        // TODO: Replace with actual email service (Azure Communication Services or SendGrid)
        // For now, logging the email (mock implementation)
        _logger.LogInformation("===== INVITATION EMAIL =====");
        _logger.LogInformation("To: {Email}", data.Email);
        _logger.LogInformation("Subject: {Subject}", emailSubject);
        _logger.LogInformation("Invitation Link: {Link}", invitationLink);
        _logger.LogInformation("Expires: {ExpiresAt}", data.ExpiresAt);
        _logger.LogInformation("============================");

        // PRODUCTION IMPLEMENTATION:
        // await _emailClient.SendAsync(
        //     WaitUntil.Completed,
        //     senderAddress: "noreply@company.com",
        //     recipientAddress: data.Email,
        //     subject: emailSubject,
        //     htmlContent: emailBody
        // );

        await Task.CompletedTask; // Placeholder
    }

    /// <summary>
    /// Build HTML email template
    /// </summary>
    private string BuildEmailHtml(InvitationEmailRequest data, string invitationLink)
    {
        var expiresAt = DateTime.Parse(data.ExpiresAt).ToString("MMMM dd, yyyy 'at' hh:mm tt");
        var companyName = data.CompanyName ?? "Our Company";

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
                            
                            <!-- Required Documents -->
                            <div style=""background-color: #e3f2fd; border-left: 4px solid #2196f3; padding: 20px; margin: 30px 0;"">
                                <h3 style=""color: #1976d2; margin: 0 0 15px 0; font-size: 18px;"">📋 What You'll Need</h3>
                                <ul style=""color: #0d47a1; margin: 0; padding-left: 20px; line-height: 1.8;"">
                                    <li>Tax ID (W-9/W-8) or VAT Number</li>
                                    <li>Legal Entity Information</li>
                                    <li>Banking Details (IBAN, Account Number)</li>
                                    <li>Certificate of Insurance</li>
                                    <li>Primary Contact Information</li>
                                </ul>
                            </div>
                            
                            <!-- Alternative Link -->
                            <p style=""color: #666666; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                If the button doesn't work, copy and paste this link into your browser:
                            </p>
                            <p style=""color: #667eea; font-size: 13px; word-break: break-all; margin: 10px 0 0 0;"">
                                {invitationLink}
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Support Section -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 30px; border-top: 1px solid #e0e0e0;"">
                            <h3 style=""color: #333333; margin: 0 0 15px 0; font-size: 16px;"">Need Help?</h3>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.6; margin: 0;"">
                                If you have any questions or encounter issues during registration, please contact our Vendor Management team:
                            </p>
                            <p style=""color: #667eea; font-size: 14px; margin: 10px 0 0 0;"">
                                📧 Email: <a href=""mailto:vendorsupport@company.com"" style=""color: #667eea; text-decoration: none;"">vendorsupport@company.com</a><br>
                                📞 Phone: +1 (555) 123-4567
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 20px; text-align: center; background-color: #333333; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0 0 10px 0;"">
                                This is an automated message. Please do not reply to this email.
                            </p>
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                <strong>Security Notice:</strong> This invitation link is unique to your email address ({data.Email}). Do not share this link with others.
                            </p>
                            <p style=""color: #666666; font-size: 11px; margin: 15px 0 0 0;"">
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
}

/// <summary>
/// Request model for invitation email
/// </summary>
public class InvitationEmailRequest
{
    public string InvitationId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
}
