using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;
using VendorMdm.Api.Services.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Mapping;
using VendorMdm.Api.Middleware;
using Microsoft.AspNetCore.Authentication;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Core.Framework.Extensions;
using VendorMdm.Core.Framework.Resilience; // Required for CorePolicyRegistry
using VendorMdm.Core.Framework.Events;
using VendorMdm.Api.Services.Events;
using VendorMdm.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- AZURE KEY VAULT CONFIGURATION ---
var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    try
    {
        var keyVaultClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        builder.Configuration.AddAzureKeyVault(keyVaultClient, new KeyVaultSecretManager());
        Console.WriteLine($"✅ Azure Key Vault configured: {keyVaultUrl}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Failed to connect to Key Vault: {ex.Message}");
        Console.WriteLine("💡 Falling back to local configuration.");
    }
}
else if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("🔧 Development mode: Using local configuration");
}

// --- DATA SOURCE MODE DETECTION ---
var dataSourceModeString = builder.Configuration["DataSourceMode"];
DataSourceMode dataSourceMode = DataSourceMode.Auto;
if (!string.IsNullOrEmpty(dataSourceModeString) && Enum.TryParse<DataSourceMode>(dataSourceModeString, true, out var parsedMode))
{
    dataSourceMode = parsedMode;
}

var useLocalEmulators = builder.Configuration.GetValue<bool>("UseLocalEmulators");
var sqlConnection = builder.Configuration.GetConnectionString("Sql") ?? builder.Configuration.GetConnectionString("DefaultConnection");
var cosmosConnection = builder.Configuration.GetConnectionString("Cosmos");
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");

// Auto-detect mode logic
if (dataSourceMode == DataSourceMode.Auto)
{
    if (!useLocalEmulators && (sqlConnection?.Contains("YOUR_") == true || 
        string.IsNullOrEmpty(sqlConnection) || 
        cosmosConnection?.Contains("YOUR_") == true))
    {
        dataSourceMode = DataSourceMode.Local;
        useLocalEmulators = true;
    }
    else if (!string.IsNullOrEmpty(sqlConnection) && !sqlConnection.Contains("YOUR_"))
    {
        dataSourceMode = DataSourceMode.Connected;
        useLocalEmulators = false;
    }
    else
    {
        dataSourceMode = DataSourceMode.Local;
        useLocalEmulators = true;
    }
}
else if (dataSourceMode == DataSourceMode.Local) useLocalEmulators = true;
else if (dataSourceMode == DataSourceMode.Connected) useLocalEmulators = false;
else if (dataSourceMode == DataSourceMode.Mock) useLocalEmulators = true;

if (useLocalEmulators || dataSourceMode == DataSourceMode.Local)
{
    sqlConnection = builder.Configuration.GetSection("LocalConnectionStrings")["Sql"];
    cosmosConnection = builder.Configuration.GetSection("LocalConnectionStrings")["Cosmos"];
    serviceBusConnection = builder.Configuration.GetSection("LocalConnectionStrings")["ServiceBus"];
}

Console.WriteLine($"Active Mode: {dataSourceMode}");

// --- SERVICES REGISTRATION ---

builder.Services.AddControllers(options =>
{
    // Global input sanitization filter (Section 7.C - Input Hygiene)
    options.Filters.Add<VendorMdm.Api.Filters.InputSanitizationActionFilter>();
})
.AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });

// API Versioning (Brain v1.2.0 compliance - api-versioning-standard.md)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.UrlSegmentApiVersionReader(),
        new Asp.Versioning.HeaderApiVersionReader("X-Api-Version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Rate Limiting
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "anonymous", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
    }));

// Observability
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("VendorMdm.Api")
        .ConfigureResource(resource => resource.AddService("VendorMdm.Api"))
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .ConfigureResource(resource => resource.AddService("VendorMdm.Api"))
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

// 1. CORE FRAMEWORK INTEGRATION
builder.Services.AddCoreFramework(builder.Configuration, "VendorMdm.Api");

// 2. APP-SPECIFIC REPOSITORIES (Core Implementations)
builder.Services.AddScoped<VendorMdm.Core.Framework.Security.Authentication.IUserRepository, VendorMdm.Api.Repositories.UserRepository>();
builder.Services.AddScoped<VendorMdm.Core.Framework.Security.Authentication.IRefreshTokenRepository, VendorMdm.Api.Repositories.RefreshTokenRepository>();
builder.Services.AddScoped<VendorMdm.Core.Framework.Security.Authorization.IUserRoleRepository, VendorMdm.Api.Repositories.UserRoleRepository>();
// RolePermissionRepository is registered by AddCoreFramework as singleton default, but if we override it:
// builder.Services.AddScoped<VendorMdm.Core.Framework.Security.Authorization.IRolePermissionRepository, VendorMdm.Api.Repositories.RolePermissionRepository>();

// 3. DATA ACCESS
builder.Services.AddDbContext<SqlDbContext>(options => {
    if (sqlConnection?.Contains("Data Source=") == true || sqlConnection?.EndsWith(".db") == true)
        options.UseSqlite(sqlConnection);
    else
        options.UseSqlServer(sqlConnection);
});

// 4. APP SERVICES
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<ISapSimulationService, SapSimulationService>(); // Removed - Duplicate/Typo

// FileStorage: Conditional registration based on UseMock config
var useFileStorageMock = builder.Configuration.GetValue<bool>("Services:FileStorage:UseMock", true);
if (useFileStorageMock)
{
    builder.Services.AddScoped<IFileStorageService, FileStorageSimulationService>();
    Console.WriteLine("📦 FileStorage: Using Simulation Service");
}
else
{
    builder.Services.AddScoped<IFileStorageService, FileStorageAzureBlobService>();
    Console.WriteLine("📦 FileStorage: Using Azure Blob Service");
} 
// builder.Services.AddScoped<VendorMdm.Api.Services.IAuthorizationService, VendorMdm.Api.Services.AuthorizationService>(); // Legacy - Removed as file not found auth migrated
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICosmosRepository, CosmosRepository>();
builder.Services.AddScoped<CosmosRepository>(); // Concrete registration: several services inject CosmosRepository directly

// Canonical / Patterns
builder.Services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ITemporalQueryService, TemporalQueryService>();
builder.Services.AddScoped<IDataMaskingService, DataMaskingService>();
builder.Services.AddScoped<IFieldEncryptionService, FieldEncryptionService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddApplicationInsightsTelemetry();

// Entity Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IFundService, FundService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVendorApplicationService, VendorApplicationService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddSingleton<ITotpService, TotpService>();

// Helper Services
builder.Services.AddSingleton<LevenshteinMatcher>();
builder.Services.AddSingleton<IbanValidator>();
builder.Services.AddSingleton<SwiftValidator>();
builder.Services.AddSingleton<SapNameValidator>();

// =====================================================
// EVENT-DRIVEN ARCHITECTURE (EDA)
// =====================================================
// Core dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Event handlers (registered for each event type they handle)
builder.Services.AddScoped<IEventHandler<VendorCreatedEvent>, SignalREventHandler>();
builder.Services.AddScoped<IEventHandler<VendorStatusChangedEvent>, SignalREventHandler>();

// Outbox processor for guaranteed delivery
builder.Services.AddHostedService<OutboxProcessor>();

// SignalR for real-time frontend updates
builder.Services.AddSignalR();
Console.WriteLine("📡 Event-Driven Architecture: Enabled (SignalR + Outbox)");

// External Integrations
builder.Services.AddAzureClients(clientBuilder => {
    if (!string.IsNullOrEmpty(serviceBusConnection)) clientBuilder.AddServiceBusClient(serviceBusConnection);
    if (!useLocalEmulators && !string.IsNullOrEmpty(serviceBusConnection) && !serviceBusConnection.Contains("SharedAccessKey"))
        clientBuilder.UseCredential(new DefaultAzureCredential());
});

if (useLocalEmulators)
{
    builder.Services.AddScoped<IServiceBusService, ServiceBusSimulationService>();
    builder.Services.AddScoped<ISapVendorService, SapVendorSimulationService>();
    builder.Services.AddScoped<ISanctionsScreeningService, SanctionsScreeningSimulationService>();
}
else
{
    builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
    builder.Services.AddScoped<ISapVendorService, SapVendorRfcService>();
    builder.Services.AddHttpClient<ISanctionsScreeningService, SanctionsScreeningOpenSanctionsService>()
        .AddPolicyHandler(CorePolicyRegistry.HttpResilientPolicy);
}

// Additional Cosmos Client
builder.Services.AddSingleton<CosmosClient>(sp => {
    if (useLocalEmulators)
    {
        var emulatorKey = builder.Configuration["CosmosEmulator:AccountKey"] ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        var emulatorEndpoint = builder.Configuration["CosmosEmulator:Endpoint"] ?? "https://localhost:8081/";
        return new CosmosClient($"AccountEndpoint={emulatorEndpoint};AccountKey={emulatorKey}");
    }
    if (!string.IsNullOrEmpty(cosmosConnection) && cosmosConnection.Contains("AccountKey")) return new CosmosClient(cosmosConnection);
    return new CosmosClient(cosmosConnection, new DefaultAzureCredential());
});

// Configure CORS (Environment-based - Section 7.B)
string[] GetAllowedOrigins(IConfiguration config, IWebHostEnvironment env)
{
    var baseUrl = config["App:BaseUrl"];

    if (env.IsProduction())
    {
        // Production: ONLY the configured BaseUrl (NO localhost)
        return string.IsNullOrEmpty(baseUrl)
            ? new[] { "https://victorious-water-095da360f.5.azurestaticapps.net" }
            : new[] { baseUrl };
    }
    else if (env.EnvironmentName == "Staging")
    {
        // Staging: BaseUrl + localhost for testing
        // Note: IsStaging() doesn't exist in ASP.NET Core - use EnvironmentName
        return new[]
        {
            baseUrl ?? "https://purple-moss-066604e03.4.azurestaticapps.net",
            "http://localhost:3000",
            "http://localhost:5173"
        };
    }
    else
    {
        // Development: localhost (range of common dev ports) + configured frontend URLs
        return new[]
        {
            "http://localhost:3000",
            "http://localhost:3001",
            "http://localhost:3002",
            "http://localhost:3003",
            "http://localhost:5173",
            "http://localhost:5174",
            "https://victorious-water-095da360f.5.azurestaticapps.net",
            "https://purple-moss-066604e03.4.azurestaticapps.net",
            "https://stvendormdmdev.blob.core.windows.net"
        };
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = GetAllowedOrigins(builder.Configuration, builder.Environment);

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

Console.WriteLine($"🌐 CORS: Allowed origins: {string.Join(", ", GetAllowedOrigins(builder.Configuration, builder.Environment))}");

// Configure Authentication
// For local development, use cookie authentication with mock middleware
// For production, Azure AD or JWT Bearer will be configured separately
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = "VendorMdmAuth";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(120); // 2h per Golden Rule 7.A
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // ApproverOnly policy: Requires Approver, MDMAdmin, or ITAdmin role
    options.AddPolicy("ApproverOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Approver") ||
            context.User.IsInRole("MDMAdmin") ||
            context.User.IsInRole("ITAdmin")));

    // RequestorOnly policy: Requires Requestor role (or higher)
    options.AddPolicy("RequestorOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Requestor") ||
            context.User.IsInRole("Approver") ||
            context.User.IsInRole("MDMAdmin") ||
            context.User.IsInRole("ITAdmin")));

    // AdminOnly policy: Requires MDMAdmin or ITAdmin role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("MDMAdmin") ||
            context.User.IsInRole("ITAdmin")));
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SqlDbContext>();

var app = builder.Build();

// --- PIPELINE CONFIGURATION ---

// DB Init
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<SqlDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        await DbInitializer.InitializeAsync(dbContext, logger);
        Console.WriteLine("✅ Database initialized.");
    }
    catch (Exception ex) { Console.WriteLine($"⚠️ DB Init Warning: {ex.Message}"); }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Security Headers (Section 7.B - MUST be BEFORE authentication)
app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Auth Middleware (Legacy + Core)
// Impersonation/GhostUser logic:
var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
if (!string.IsNullOrEmpty(azureAdClientId))
{
    app.UseAuthentication();
    app.UseMiddleware<ImpersonationMiddleware>();
    app.UseGhostUserBlocking(); 
}
else
{
    // If no Azure AD, assume local dev or Core Auth
    app.UseAuthentication();
}

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<MockAuthMiddleware>();
}

app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

// SignalR Hub endpoint (Event-Driven Architecture)
app.MapHub<EventHub>("/hubs/events").RequireCors("AllowFrontend");

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/startup");

// Email check log
Console.WriteLine($"📧 Email Service: {(builder.Configuration.GetValue<bool>("EmailService:Smtp:Enabled") ? "SMTP Configured" : "Mock/Unconfigured")}");

app.Run();
