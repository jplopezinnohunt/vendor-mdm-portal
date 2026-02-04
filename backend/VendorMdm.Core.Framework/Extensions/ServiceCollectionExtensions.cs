using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Json;
using VendorMdm.Core.Framework.Logging;
using VendorMdm.Core.Framework.Resilience;
using VendorMdm.Core.Framework.Security;

namespace VendorMdm.Core.Framework.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to add Core.Framework services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Core.Framework services to the service collection.
    /// This is the ONE LINE that apps use to get all core functionality.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="appName">Application name (VendorMDM, EmployeeMDM, etc.)</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCoreFramework(
        this IServiceCollection services,
        IConfiguration configuration,
        string appName,
        Action<CoreFrameworkOptions>? configureOptions = null)
    {
        // Create options
        var options = new CoreFrameworkOptions();
        configureOptions?.Invoke(options);

        // 1. Logging (Serilog)
        services.AddLogging(options, appName, configuration);

        // 2. Resilience (Polly)
        services.AddResilience(options);

        // 3. Health Checks
        services.AddHealthChecks(options, configuration);

        // 4. File Storage
        services.AddFileStorage(options, configuration);

        // 5. Observability (OpenTelemetry)
        if (options.EnableDistributedTracing)
        {
            services.AddObservability(options, appName, configuration);
        }

        // 6. Security (if enabled)
        if (options.EnableSecurity)
        {
            services.AddSecurity(options, configuration);
        }

        return services;
    }

    private static void AddLogging(
        this IServiceCollection services,
        CoreFrameworkOptions options,
        string appName,
        IConfiguration configuration)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("AppName", appName)
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .WriteTo.Console(new JsonFormatter())
            .WriteTo.ApplicationInsights(
                configuration["ApplicationInsights:ConnectionString"] ?? "",
                TelemetryConverter.Traces)
            .CreateLogger();

        // Register structured logger
        services.AddSingleton<IStructuredLogger>(sp => 
            new SerilogStructuredLogger(Log.Logger, appName));
    }

    private static void AddResilience(
        this IServiceCollection services,
        CoreFrameworkOptions options)
    {
        // Register resilience policy service
        services.AddSingleton<IResiliencePolicy, ResiliencePolicyService>();

        // Add HTTP client with resilience policies
        services.AddHttpClient("resilient")
            .AddPolicyHandler(CorePolicyRegistry.HttpRetryPolicy)
            .AddPolicyHandler(CorePolicyRegistry.HttpCircuitBreakerPolicy);
    }

    private static void AddHealthChecks(
        this IServiceCollection services,
        CoreFrameworkOptions options,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Database health check (if enabled)
        if (options.EnableDatabaseHealthCheck)
        {
            var connString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connString))
            {
                healthChecksBuilder.AddSqlServer(
                    connString,
                    name: "database",
                    tags: new[] { "ready" });
            }
        }

        // Blob storage health check (if enabled)
        if (options.EnableBlobStorageHealthCheck)
        {
            var blobConnString = configuration["Azure:BlobStorage:ConnectionString"];
            if (!string.IsNullOrEmpty(blobConnString))
            {
                healthChecksBuilder.AddAzureBlobStorage(
                    blobConnString,
                    name: "blob-storage",
                    tags: new[] { "ready" });
            }
        }

        // Service bus health check (if enabled)
        if (options.EnableServiceBusHealthCheck)
        {
            var sbConnString = configuration["Azure:ServiceBus:ConnectionString"];
            if (!string.IsNullOrEmpty(sbConnString))
            {
                healthChecksBuilder.AddAzureServiceBusTopic(
                    sbConnString,
                    "events",
                    name: "service-bus",
                    tags: new[] { "ready" });
            }
        }
    }

    private static void AddFileStorage(
        this IServiceCollection services,
        CoreFrameworkOptions options,
        IConfiguration configuration)
    {
        // Register Azure Blob Storage service
        var blobConnectionString = configuration["Azure:BlobStorage:ConnectionString"];
        if (!string.IsNullOrEmpty(blobConnectionString))
        {
            services.AddSingleton(sp => new Azure.Storage.Blobs.BlobServiceClient(blobConnectionString));
            services.AddSingleton(new FileSystem.AzureBlobStorageOptions 
            { 
                ConnectionString = blobConnectionString,
                DefaultContainer = configuration["Azure:BlobStorage:DefaultContainer"]
            });
            services.AddSingleton<FileSystem.IFileStorageService, FileSystem.AzureBlobStorageService>();
        }

        // Register Core Health Check Service
        services.AddSingleton<HealthChecks.IHealthCheckService, HealthChecks.CoreHealthCheckService>();
    }

    private static void AddObservability(
        this IServiceCollection services,
        CoreFrameworkOptions options,
        string appName,
        IConfiguration configuration)
    {
        // Register distributed tracing service
        services.AddSingleton<Observability.IDistributedTracing>(sp => 
            new Observability.DistributedTracingService(appName));

        // Register metrics service
        services.AddSingleton<Observability.IMetricsService>(sp => 
            new Observability.MetricsService(appName));

        // Note: Full OpenTelemetry configuration will be added when apps integrate Core.Framework
        // For now, services are registered and ready to use
        // Apps can configure OpenTelemetry in their Program.cs using:
        // services.AddOpenTelemetry()
        //   .WithTracing(...)
        //   .WithMetrics(...)
    }

    private static void AddSecurity(
        this IServiceCollection services,
        CoreFrameworkOptions options,
        IConfiguration configuration)
    {
        // Input Sanitization (Section 7.C - Input Hygiene)
        services.AddSingleton<IInputSanitizer, InputSanitizer>();

        // JWT Authentication Service
        var jwtOptions = new Security.Authentication.JwtAuthenticationOptions
        {
            SecretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required"),
            Issuer = configuration["Jwt:Issuer"] ?? "VendorMDM",
            Audience = configuration["Jwt:Audience"] ?? "VendorMDM",
            AccessTokenExpirationMinutes = int.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15"),
            RefreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "30")
        };
        services.AddSingleton(jwtOptions);

        // Note: Apps must register IUserRepository and IRefreshTokenRepository
        // services.AddScoped<Security.Authentication.IAuthenticationService, Security.Authentication.JwtAuthenticationService>();

        // Role-Based Authorization Service
        services.AddSingleton<Security.Authorization.IRolePermissionRepository, Security.Authorization.DefaultRolePermissionRepository>();

        // Note: Apps must register IUserRoleRepository
        // services.AddScoped<Security.Authorization.IAuthorizationService, Security.Authorization.RoleBasedAuthorizationService>();
    }
}

/// <summary>
/// Configuration options for Core.Framework.
/// </summary>
public class CoreFrameworkOptions
{
    /// <summary>
    /// Enable distributed tracing with OpenTelemetry.
    /// Default: true
    /// </summary>
    public bool EnableDistributedTracing { get; set; } = true;

    /// <summary>
    /// Enable security services (authentication, authorization).
    /// Default: true
    /// </summary>
    public bool EnableSecurity { get; set; } = true;

    /// <summary>
    /// Enable database health check.
    /// Default: true
    /// </summary>
    public bool EnableDatabaseHealthCheck { get; set; } = true;

    /// <summary>
    /// Enable blob storage health check.
    /// Default: true
    /// </summary>
    public bool EnableBlobStorageHealthCheck { get; set; } = true;

    /// <summary>
    /// Enable service bus health check.
    /// Default: true
    /// </summary>
    public bool EnableServiceBusHealthCheck { get; set; } = true;

    /// <summary>
    /// Log level for structured logging.
    /// Default: Information
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Cache provider (InMemory, Redis).
    /// Default: InMemory
    /// </summary>
    public CacheProvider CacheProvider { get; set; } = CacheProvider.InMemory;
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

public enum CacheProvider
{
    InMemory,
    Redis
}
