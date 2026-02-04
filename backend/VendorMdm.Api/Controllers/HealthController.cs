using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VendorMdm.Api.Services;

namespace VendorMdm.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;
    private readonly IEmailService _emailService;

    public HealthController(IConfiguration configuration, ILogger<HealthController> logger, IEmailService emailService)
    {
        _configuration = configuration;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Get email service configuration status
    /// </summary>
    [HttpGet("email-service")]
    public async Task<IActionResult> GetEmailServiceStatus()
    {
        var useLocalEmulators = _configuration.GetValue<bool>("UseLocalEmulators");
        var smtpEnabled = _configuration.GetValue<bool>("EmailService:Smtp:Enabled", false);
        
        // Check SMTP configuration
        var smtpHost = _configuration["EmailService:Smtp:Host"] 
            ?? _configuration["EmailService__Smtp__Host"];
        var smtpUsername = _configuration["EmailService:Smtp:Username"] 
            ?? _configuration["EmailService__Smtp__Username"];
        var smtpPassword = _configuration["EmailService:Smtp:Password"] 
            ?? _configuration["EmailService__Smtp__Password"];
        var smtpFromEmail = _configuration["EmailService:Smtp:FromEmail"] 
            ?? _configuration["EmailService__Smtp__FromEmail"];

        var smtpConfigured = smtpEnabled 
            && !string.IsNullOrEmpty(smtpHost)
            && !string.IsNullOrEmpty(smtpUsername)
            && !string.IsNullOrEmpty(smtpPassword)
            && !string.IsNullOrEmpty(smtpFromEmail);

        // Check Azure Function availability
        var functionUrl = _configuration["EmailService:FunctionUrl"];
        var azureFunctionConfigured = !string.IsNullOrEmpty(functionUrl);

        // Determine email mode
        string emailMode;
        string status;
        string message;

        if (useLocalEmulators)
        {
            // Local development
            if (smtpConfigured)
            {
                emailMode = "smtp";
                status = "configured";
                message = $"SMTP configured ({smtpFromEmail})";
            }
            else if (azureFunctionConfigured)
            {
                emailMode = "azure-function";
                status = "configured";
                message = "Azure Function endpoint configured";
            }
            else
            {
                emailMode = "console-logging";
                status = "fallback";
                message = "Email logging to console (SMTP not configured)";
            }
        }
        else
        {
            // Production/Azure
            if (azureFunctionConfigured)
            {
                emailMode = "azure-communication";
                status = "configured";
                message = "Azure Communication Services";
            }
            else if (smtpConfigured)
            {
                emailMode = "smtp";
                status = "configured";
                message = "SMTP configured";
            }
            else
            {
                emailMode = "none";
                status = "not-configured";
                message = "Email service not configured";
            }
        }

        return Ok(new
        {
            emailService = new
            {
                mode = emailMode,
                status = status,
                message = message,
                environment = useLocalEmulators ? "local" : "azure",
                configured = smtpConfigured || azureFunctionConfigured,
                connected = await _emailService.TestConnectionAsync(),
                smtp = new
                {
                    enabled = smtpEnabled,
                    configured = smtpConfigured,
                    host = smtpHost,
                    fromEmail = smtpFromEmail
                },
                azureFunction = new
                {
                    configured = azureFunctionConfigured
                }
            }
        });
    }

    /// <summary>
    /// General health check
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = _configuration.GetValue<bool>("UseLocalEmulators") ? "local" : "azure"
        });
    }
}
