namespace VendorMdm.Shared.Models;

/// <summary>
/// Defines the data source mode for the application.
/// </summary>
public enum DataSourceMode
{
    /// <summary>
    /// Auto-detect based on connection strings and environment.
    /// Falls back to Local if Azure connection strings are not configured.
    /// </summary>
    Auto = 0,
    
    /// <summary>
    /// Local development mode using SQLite database and local emulators.
    /// </summary>
    Local = 1,
    
    /// <summary>
    /// Mock mode for testing - no real connections, returns mock/empty data.
    /// </summary>
    Mock = 2,
    
    /// <summary>
    /// Connected mode using full Azure resources (SQL Server, Cosmos DB, Service Bus).
    /// </summary>
    Connected = 3
}

/// <summary>
/// Overall data source configuration and status.
/// </summary>
public class DataSourceConfiguration
{
    public DataSourceMode Mode { get; set; }
    public DatabaseConfig Database { get; set; } = new();
    public CosmosConfig Cosmos { get; set; } = new();
    public ServiceBusConfig ServiceBus { get; set; } = new();
    public EmailConfig Email { get; set; } = new();
    public SapConfig Sap { get; set; } = new();
    public FileStorageConfig FileStorage { get; set; } = new();
    public SanctionsConfig Sanctions { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

/// <summary>
/// Database connection configuration and status.
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// Database type: "SQLite", "SqlServer", or "Mock"
    /// </summary>
    public string Type { get; set; } = "Unknown";
    
    /// <summary>
    /// Connection string (sanitized for display - no credentials)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the database is currently connected
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Current mode for this service
    /// </summary>
    public string Mode { get; set; } = "Unknown";
    
    /// <summary>
    /// Additional status information
    /// </summary>
    public string? StatusMessage { get; set; }
}

/// <summary>
/// Cosmos DB configuration and status.
/// </summary>
public class CosmosConfig
{
    /// <summary>
    /// Cosmos DB endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether Cosmos DB is currently connected
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Current mode for this service
    /// </summary>
    public string Mode { get; set; } = "Unknown";
    
    /// <summary>
    /// Database name being used
    /// </summary>
    public string? DatabaseName { get; set; }
    
    /// <summary>
    /// Additional status information
    /// </summary>
    public string? StatusMessage { get; set; }
}

/// <summary>
/// Service Bus configuration and status.
/// </summary>
public class ServiceBusConfig
{
    /// <summary>
    /// Service Bus namespace
    /// </summary>
    public string Namespace { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether Service Bus is currently connected
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Current mode for this service
    /// </summary>
    public string Mode { get; set; } = "Unknown";
    
    /// <summary>
    /// Additional status information
    /// </summary>
    public string? StatusMessage { get; set; }
}

/// <summary>
/// Email service configuration and status.
/// </summary>
public class EmailConfig
{
    /// <summary>
    /// Email mode: "Console", "SMTP", "AzureCommunication", or "Mock"
    /// </summary>
    public string Mode { get; set; } = "Unknown";
    
    public bool IsConfigured { get; set; }
    
    /// <summary>
    /// Whether email service is currently connected/online
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// SMTP host if using SMTP
    /// </summary>
    public string? SmtpHost { get; set; }

    /// <summary>
    /// From email address
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// Additional status information
    /// </summary>
    public string? StatusMessage { get; set; }
}

public class SapConfig
{
    public string Mode { get; set; } = "Unknown";
    public bool IsConnected { get; set; }
    public string? StatusMessage { get; set; }
}

public class FileStorageConfig
{
    public string Mode { get; set; } = "Unknown";
    public bool IsConnected { get; set; }
    public string? StatusMessage { get; set; }
}

public class SanctionsConfig
{
    public string Mode { get; set; } = "Unknown";
    public bool IsConnected { get; set; }
    public string? StatusMessage { get; set; }
}
