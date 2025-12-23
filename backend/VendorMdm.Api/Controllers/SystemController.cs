using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.Azure.Cosmos;

namespace VendorMdm.Api.Controllers;

/// <summary>
/// System administration and monitoring endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class SystemController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemController> _logger;
    private readonly SqlDbContext _dbContext;
    private readonly CosmosClient? _cosmosClient;

    public SystemController(
        IConfiguration configuration,
        ILogger<SystemController> logger,
        SqlDbContext dbContext,
        CosmosClient? cosmosClient = null)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
        _cosmosClient = cosmosClient;
    }

    /// <summary>
    /// Get current data source configuration and connection status.
    /// </summary>
    [HttpGet("data-sources")]
    public async Task<IActionResult> GetDataSourceStatus()
    {
        try
        {
            var config = new DataSourceConfiguration
            {
                Mode = GetCurrentMode(),
                Database = await GetDatabaseStatusAsync(),
                Cosmos = await GetCosmosStatusAsync(),
                ServiceBus = GetServiceBusStatus(),
                Email = GetEmailStatus(),
                LastChecked = DateTime.UtcNow
            };

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data source status");
            return StatusCode(500, new { error = "Failed to retrieve data source status" });
        }
    }

    /// <summary>
    /// Get service implementation status (Mock vs Real)
    /// Shows which services are using Mock implementations vs Real integrations
    /// </summary>
    [HttpGet("services")]
    public IActionResult GetServiceStatus()
    {
        try
        {
            var environment = DetermineEnvironment();
            
            var serviceStatus = new
            {
                Environment = environment,
                Description = GetEnvironmentDescription(environment),
                Services = new
                {
                    SAP = GetServiceConfig("Services:SAP"),
                    FileStorage = GetServiceConfig("Services:FileStorage"),
                    SanctionsScreening = GetServiceConfig("Services:SanctionsScreening"),
                    RBAC = GetServiceConfig("Services:RBAC"),
                    MasterData = GetServiceConfig("Services:MasterData"),
                    Workflow = GetServiceConfig("Services:Workflow"),
                    Email = GetServiceConfig("Services:Email")
                },
                LastChecked = DateTime.UtcNow
            };

            return Ok(serviceStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service status");
            return StatusCode(500, new { error = "Failed to retrieve service status" });
        }
    }

    private string DetermineEnvironment()
    {
        var dataSourceMode = GetCurrentMode();
        
        // Check if running locally
        var isLocal = dataSourceMode == DataSourceMode.Local ||
                     _configuration.GetValue<bool>("UseLocalEmulators");
        
        if (isLocal)
        {
            return "Local";
        }

        // Check if in Azure
        var isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
        
        if (isAzure)
        {
            // Check if any service is using Mock in Azure
            var sapMock = _configuration.GetValue<bool>("Services:SAP:UseMock", true);
            var filesMock = _configuration.GetValue<bool>("Services:FileStorage:UseMock", true);
            var sanctionsMock = _configuration.GetValue<bool>("Services:SanctionsScreening:UseMock", true);
            
            var anyMock = sapMock || filesMock || sanctionsMock;
            
            return anyMock ? "Azure (Mock)" : "Azure (Real)";
        }

        return "Unknown";
    }

    private string GetEnvironmentDescription(string environment)
    {
        return environment switch
        {
            "Local" => "Running locally with Mock services for development",
            "Azure (Mock)" => "Deployed to Azure using Mock services for testing",
            "Azure (Real)" => "Deployed to Azure with Real service integrations",
            _ => "Environment Unknown"
        };
    }

    private object GetServiceConfig(string configPath)
    {
        var useMock = _configuration.GetValue<bool>($"{configPath}:UseMock", true);
        var realProvider = _configuration[$"{configPath}:RealProvider"] ?? "Not Configured";
        
        return new
        {
            Mode = useMock ? "Mock" : "Real",
            Implementation = useMock ? "Simulation/Hardcoded" : realProvider,
            ConfigPath = configPath
        };
    }

    private DataSourceMode GetCurrentMode()
    {
        var modeString = _configuration["DataSourceMode"];
        
        if (string.IsNullOrEmpty(modeString))
        {
            // Backward compatibility: check UseLocalEmulators
            var useLocalEmulators = _configuration.GetValue<bool>("UseLocalEmulators");
            return useLocalEmulators ? DataSourceMode.Local : DataSourceMode.Auto;
        }

        if (Enum.TryParse<DataSourceMode>(modeString, true, out var mode))
        {
            return mode;
        }

        return DataSourceMode.Auto;
    }

    private async Task<DatabaseConfig> GetDatabaseStatusAsync()
    {
        var config = new DatabaseConfig();
        
        try
        {
            var connectionString = _dbContext.Database.GetConnectionString() ?? string.Empty;
            
            // Determine database type
            if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
            {
                config.Type = "SQLite";
                config.Mode = "Local";
                config.ConnectionString = SanitizeConnectionString(connectionString);
            }
            else if (connectionString.Contains("database.windows.net"))
            {
                config.Type = "SQL Server";
                config.Mode = "Connected";
                config.ConnectionString = SanitizeConnectionString(connectionString);
            }
            else
            {
                config.Type = "Unknown";
                config.Mode = "Unknown";
            }

            // Test connection
            config.IsConnected = await _dbContext.Database.CanConnectAsync();
            config.StatusMessage = config.IsConnected ? "Connected" : "Not connected";
        }
        catch (Exception ex)
        {
            config.IsConnected = false;
            config.StatusMessage = $"Error: {ex.Message}";
            _logger.LogWarning(ex, "Failed to check database connection");
        }

        return config;
    }

    private async Task<CosmosConfig> GetCosmosStatusAsync()
    {
        var config = new CosmosConfig();
        
        try
        {
            var cosmosConnection = _configuration.GetConnectionString("Cosmos");
            
            if (string.IsNullOrEmpty(cosmosConnection))
            {
                config.Mode = "Not Configured";
                config.StatusMessage = "No connection string configured";
                return config;
            }

            // Extract endpoint from connection string
            if (cosmosConnection.StartsWith("https://"))
            {
                var endpointUri = new Uri(cosmosConnection.Split(';')[0]);
                config.Endpoint = endpointUri.Host;
            }
            else
            {
                config.Endpoint = cosmosConnection;
            }

            // Check if using emulator
            if (cosmosConnection.Contains("localhost") || cosmosConnection.Contains("127.0.0.1"))
            {
                config.Mode = "Local Emulator";
            }
            else if (cosmosConnection.Contains("documents.azure.com"))
            {
                config.Mode = "Connected";
            }
            else
            {
                config.Mode = "Unknown";
            }

            // Test connection if client is available
            if (_cosmosClient != null)
            {
                try
                {
                    var databases = _cosmosClient.GetDatabaseQueryIterator<string>("SELECT VALUE c.id FROM c");
                    config.IsConnected = true;
                    config.StatusMessage = "Connected";
                }
                catch
                {
                    config.IsConnected = false;
                    config.StatusMessage = "Client available but not connected";
                }
            }
            else
            {
                config.IsConnected = false;
                config.StatusMessage = "Client not initialized";
            }
        }
        catch (Exception ex)
        {
            config.IsConnected = false;
            config.StatusMessage = $"Error: {ex.Message}";
            _logger.LogWarning(ex, "Failed to check Cosmos DB connection");
        }

        return config;
    }

    private ServiceBusConfig GetServiceBusStatus()
    {
        var config = new ServiceBusConfig();
        
        try
        {
            var serviceBusConnection = _configuration.GetConnectionString("ServiceBus");
            
            if (string.IsNullOrEmpty(serviceBusConnection))
            {
                config.Mode = "Not Configured";
                config.StatusMessage = "No connection string configured";
                return config;
            }

            // Extract namespace
            if (serviceBusConnection.Contains(".servicebus.windows.net"))
            {
                config.Namespace = serviceBusConnection.Split('.')[0];
                config.Mode = "Connected";
            }
            else if (serviceBusConnection.Contains("localhost"))
            {
                config.Namespace = "localhost";
                config.Mode = "Local Emulator";
            }
            else
            {
                config.Namespace = serviceBusConnection;
                config.Mode = "Unknown";
            }

            // Note: We can't easily test Service Bus connection without sending a message
            // So we just indicate if it's configured
            config.IsConnected = !string.IsNullOrEmpty(serviceBusConnection);
            config.StatusMessage = config.IsConnected ? "Configured" : "Not configured";
        }
        catch (Exception ex)
        {
            config.IsConnected = false;
            config.StatusMessage = $"Error: {ex.Message}";
            _logger.LogWarning(ex, "Failed to check Service Bus configuration");
        }

        return config;
    }

    private EmailConfig GetEmailStatus()
    {
        var config = new EmailConfig();
        
        try
        {
            var useLocalEmulators = _configuration.GetValue<bool>("UseLocalEmulators");
            var smtpEnabled = _configuration.GetValue<bool>("EmailService:Smtp:Enabled", false);
            
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

            var functionUrl = _configuration["EmailService:FunctionUrl"];
            var azureFunctionConfigured = !string.IsNullOrEmpty(functionUrl);

            // Determine email mode
            if (useLocalEmulators)
            {
                if (smtpConfigured)
                {
                    config.Mode = "SMTP";
                    config.IsConfigured = true;
                    config.SmtpHost = smtpHost;
                    config.FromEmail = smtpFromEmail;
                    config.StatusMessage = $"SMTP configured ({smtpFromEmail})";
                }
                else if (azureFunctionConfigured)
                {
                    config.Mode = "Azure Function";
                    config.IsConfigured = true;
                    config.StatusMessage = "Azure Function endpoint configured";
                }
                else
                {
                    config.Mode = "Console Logging";
                    config.IsConfigured = false;
                    config.StatusMessage = "Emails logged to console (SMTP not configured)";
                }
            }
            else
            {
                if (azureFunctionConfigured)
                {
                    config.Mode = "Azure Communication";
                    config.IsConfigured = true;
                    config.StatusMessage = "Azure Communication Services configured";
                }
                else if (smtpConfigured)
                {
                    config.Mode = "SMTP";
                    config.IsConfigured = true;
                    config.SmtpHost = smtpHost;
                    config.FromEmail = smtpFromEmail;
                    config.StatusMessage = "SMTP configured";
                }
                else
                {
                    config.Mode = "Not Configured";
                    config.IsConfigured = false;
                    config.StatusMessage = "Email service not configured";
                }
            }
        }
        catch (Exception ex)
        {
            config.IsConfigured = false;
            config.StatusMessage = $"Error: {ex.Message}";
            _logger.LogWarning(ex, "Failed to check email configuration");
        }

        return config;
    }

    /// <summary>
    /// Sanitize connection string for display (remove credentials).
    /// </summary>
    private string SanitizeConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return string.Empty;

        // For SQLite, just show the file path
        if (connectionString.Contains("Data Source="))
        {
            var parts = connectionString.Split(';');
            var dataSource = parts.FirstOrDefault(p => p.Trim().StartsWith("Data Source="));
            return dataSource ?? connectionString;
        }

        // For SQL Server, remove password
        var sanitized = connectionString;
        var passwordPatterns = new[] { "Password=", "Pwd=" };
        
        foreach (var pattern in passwordPatterns)
        {
            var index = sanitized.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var endIndex = sanitized.IndexOf(';', index);
                if (endIndex > index)
                {
                    sanitized = sanitized.Remove(index, endIndex - index)
                        .Insert(index, pattern + "***");
                }
            }
        }

        return sanitized;
    }
}
