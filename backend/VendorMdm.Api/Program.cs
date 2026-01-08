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

var builder = WebApplication.CreateBuilder(args);

// --- AZURE KEY VAULT CONFIGURATION ---
// Load secrets from Azure Key Vault if configured (for production/staging)
var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl) && !builder.Environment.IsDevelopment())
{
    try
    {
        var keyVaultClient = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
        
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
    Console.WriteLine("🔧 Development mode: Using local configuration (appsettings.Development.json or User Secrets)");
}

// --- DATA SOURCE MODE DETECTION ---
var dataSourceModeString = builder.Configuration["DataSourceMode"];
DataSourceMode dataSourceMode = DataSourceMode.Auto;

if (!string.IsNullOrEmpty(dataSourceModeString) && 
    Enum.TryParse<DataSourceMode>(dataSourceModeString, true, out var parsedMode))
{
    dataSourceMode = parsedMode;
}

Console.WriteLine("");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("🔌 DATA SOURCE MODE CONFIGURATION");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine($"Configured Mode: {dataSourceMode}");


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication & Authorization
// Only enable Azure AD authentication if ClientId is configured
var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
if (!string.IsNullOrEmpty(azureAdClientId))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    
    Console.WriteLine("✅ Azure AD Authentication enabled");
}
else
{
    Console.WriteLine("⚠️  Authentication DISABLED (Azure AD not configured)");
    Console.WriteLine("   For local development only - configure AzureAd:ClientId for production");
}

// Always configure authorization policies (needed even without authentication for local dev)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrApprover", policy =>
    {
        if (!string.IsNullOrEmpty(azureAdClientId))
        {
            // Production: require roles
            policy.RequireRole("Admin", "Approver");
        }
        else
        {
            // Local dev without auth: allow all requests
            policy.RequireAssertion(context => true);
        }
    });

    options.AddPolicy("ApproverOnly", policy =>
    {
        if (!string.IsNullOrEmpty(azureAdClientId))
        {
            // Production: require Approver role strictly
            policy.RequireRole("Approver");
        }
        else
        {
            // Local dev without auth: allow all requests
            policy.RequireAssertion(context => true);
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            var allowedOrigins = new List<string> 
            { 
                "http://localhost:5173", 
                "http://localhost:3000", 
                "http://localhost:3002",
                "https://swa-vendor-mdm-dev.azurestaticapps.net",
                "https://thankful-field-0258f8110.3.azurestaticapps.net"
            };
            
            var configuredUrl = builder.Configuration["App:BaseUrl"];
            if (!string.IsNullOrEmpty(configuredUrl))
            {
                allowedOrigins.Add(configuredUrl);
            }

            policy.WithOrigins(allowedOrigins.Distinct().ToArray())

                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// --- CONNECTION STRING LOGIC ---
var useLocalEmulators = builder.Configuration.GetValue<bool>("UseLocalEmulators");
var sqlConnection = builder.Configuration.GetConnectionString("Sql");
var cosmosConnection = builder.Configuration.GetConnectionString("Cosmos");
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");

// Determine actual mode based on configuration and connection strings
if (dataSourceMode == DataSourceMode.Auto)
{
    // Auto-fallback: If Azure connection strings are still placeholders, use local emulators
    if (!useLocalEmulators && (sqlConnection?.Contains("YOUR_") == true || 
        sqlConnection?.Contains("YOUR_SERVER_NAME") == true || 
        cosmosConnection?.Contains("YOUR_") == true ||
        serviceBusConnection?.Contains("YOUR_") == true ||
        string.IsNullOrEmpty(sqlConnection)))
    {
        Console.WriteLine("⚠️  Azure connection strings contain placeholders.");
        Console.WriteLine("💡 Auto-detecting mode: Falling back to Local");
        dataSourceMode = DataSourceMode.Local;
        useLocalEmulators = true;
    }
    else if (!string.IsNullOrEmpty(sqlConnection) && !sqlConnection.Contains("YOUR_"))
    {
        Console.WriteLine("☁️  Azure connection strings detected.");
        Console.WriteLine("💡 Auto-detecting mode: Using Connected");
        dataSourceMode = DataSourceMode.Connected;
        useLocalEmulators = false;
    }
    else
    {
        Console.WriteLine("🔧 No Azure resources configured.");
        Console.WriteLine("💡 Auto-detecting mode: Using Local");
        dataSourceMode = DataSourceMode.Local;
        useLocalEmulators = true;
    }
}
else if (dataSourceMode == DataSourceMode.Local)
{
    useLocalEmulators = true;
}
else if (dataSourceMode == DataSourceMode.Connected)
{
    useLocalEmulators = false;
}
else if (dataSourceMode == DataSourceMode.Mock)
{
    useLocalEmulators = true; // Use local for mock mode
    Console.WriteLine("🧪 Mock mode enabled - services will return mock data");
}

Console.WriteLine($"Active Mode: {dataSourceMode}");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("");

if (useLocalEmulators || dataSourceMode == DataSourceMode.Local)
{
    Console.WriteLine("🔧 Using Local Emulators for development.");
    sqlConnection = builder.Configuration.GetSection("LocalConnectionStrings")["Sql"];
    cosmosConnection = builder.Configuration.GetSection("LocalConnectionStrings")["Cosmos"];
    serviceBusConnection = builder.Configuration.GetSection("LocalConnectionStrings")["ServiceBus"];
    
    Console.WriteLine($"   Database: SQLite");
    Console.WriteLine($"   Cosmos: Local Emulator");
    Console.WriteLine($"   Service Bus: Local Emulator");
}
else
{
    Console.WriteLine("☁️  Using Azure Resources.");
    Console.WriteLine($"   Database: SQL Server");
    Console.WriteLine($"   Cosmos: {cosmosConnection?.Split('/')[2] ?? "Not configured"}");
    Console.WriteLine($"   Service Bus: {serviceBusConnection?.Split('.')[0] ?? "Not configured"}");
}
Console.WriteLine("");


// 1. Azure Clients
builder.Services.AddAzureClients(clientBuilder =>
{
    // Service Bus
    clientBuilder.AddServiceBusClient(serviceBusConnection);
    
    // Default Credential (only needed for Azure Identity, not connection strings with keys)
    if (!useLocalEmulators && !serviceBusConnection.Contains("SharedAccessKey"))
    {
        clientBuilder.UseCredential(new DefaultAzureCredential());
    }
});

// 2. SQL Database
builder.Services.AddDbContext<SqlDbContext>(options =>
{
    if (sqlConnection.Contains("Data Source=") || sqlConnection.EndsWith(".db"))
    {
        // Use SQLite for local development on macOS
        options.UseSqlite(sqlConnection);
    }
    else
    {
        // Use SQL Server for Azure or Windows
        options.UseSqlServer(sqlConnection);
    }
});

// 3. Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    if (useLocalEmulators)
    {
        // Emulator usually uses a key, not Managed Identity
        return new CosmosClient(cosmosConnection);
    }
    else
    {
        // Azure: Use Managed Identity if no key in connection string, otherwise use string
        if (cosmosConnection.Contains("AccountKey"))
        {
            return new CosmosClient(cosmosConnection);
        }
        return new CosmosClient(cosmosConnection, new DefaultAzureCredential());
    }
});

// 4. Custom Services
builder.Services.AddScoped<CosmosRepository>();
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
builder.Services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
builder.Services.AddHttpClient(); // For EmailService HTTP client
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddApplicationInsightsTelemetry();

// Canonical Model Services - External System Integration
builder.Services.AddScoped<IExternalSystemMappingService, ExternalSystemMappingService>();
builder.Services.AddScoped<ISapIdMappingService, SapIdMappingService>();
builder.Services.AddScoped<IVendorSapMapper, VendorSapMapper>();

// Canonical Model Services - New Entities
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IFundService, FundService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// ═══════════════════════════════════════════════════════════════
// SIMULATION SERVICES - Mock vs Real Implementation Selection
// ═══════════════════════════════════════════════════════════════

Console.WriteLine("");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("🔌 SERVICE IMPLEMENTATION CONFIGURATION");
Console.WriteLine("═══════════════════════════════════════════════════════════");

// Helper Services (Always registered - used by both Mock and Real)
builder.Services.AddSingleton<LevenshteinMatcher>();
builder.Services.AddSingleton<IbanValidator>();
builder.Services.AddSingleton<SwiftValidator>();
builder.Services.AddSingleton<SapNameValidator>();

// SAP Services
var useSapMock = builder.Configuration.GetValue<bool>("Services:SAP:UseMock", true);
if (useSapMock)
{
    builder.Services.AddScoped<ISapVendorService, SapVendorSimulationService>();
    Console.WriteLine("✓ SAP: MOCK (Simulation with in-memory data)");
}
else
{
    builder.Services.AddScoped<ISapVendorService, SapVendorRfcService>();
    Console.WriteLine("✓ SAP: REAL (RFC connection - requires SAP NCo)");
}

// TODO: Uncomment when RBAC services are created
/*
// RBAC Services
var useRbacMock = builder.Configuration.GetValue<bool>("Services:RBAC:UseMock", true);
if (useRbacMock)
{
    builder.Services.AddScoped<IAuthorizationService, AuthorizationSimulationService>();
    builder.Services.AddScoped<IUserService, UserSimulationService>();
    Console.WriteLine("✓ RBAC: MOCK (In-memory user roles)");
}
else
{
    builder.Services.AddScoped<IAuthorizationService, AzureAdAuthorizationService>();
    builder.Services.AddScoped<IUserService, GraphUserService>();
    Console.WriteLine("✓ RBAC: REAL (Azure AD integration)");
}
*/

// TODO: Uncomment when MasterData services are created
/*
// Master Data Services
var useMasterDataMock = builder.Configuration.GetValue<bool>("Services:MasterData:UseMock", true);
if (useMasterDataMock)
{
    builder.Services.AddSingleton<IMasterDataService, MasterDataSimulationService>();
    Console.WriteLine("✓ Master Data: MOCK (In-memory countries, currencies, etc.)");
}
else
{
    builder.Services.AddScoped<IMasterDataService, DatabaseMasterDataService>();
    Console.WriteLine("✓ Master Data: REAL (SQL Database)");
}
*/

// File Storage Service
var useFileStorageMock = builder.Configuration.GetValue<bool>("Services:FileStorage:UseMock", true);
if (useFileStorageMock)
{
    builder.Services.AddScoped<IFileStorageService, FileStorageSimulationService>();
    Console.WriteLine("✓ File Storage: MOCK (Local filesystem)");
}
else
{
    builder.Services.AddScoped<IFileStorageService, FileStorageAzureBlobService>();
    Console.WriteLine("✓ File Storage: REAL (Azure Blob Storage - requires configuration)");
}

// Sanctions Screening Service
var useSanctionsScreeningMock = builder.Configuration.GetValue<bool>("Services:SanctionsScreening:UseMock", true);
if (useSanctionsScreeningMock)
{
    builder.Services.AddScoped<ISanctionsScreeningService, SanctionsScreeningSimulationService>();
    Console.WriteLine("✓ Sanctions Screening: MOCK (Hardcoded test cases)");
}
else
{
    var realProvider = builder.Configuration["Services:SanctionsScreening:RealProvider"];
    
    if (realProvider == "OfacSource")
    {
        // Register HttpClient for OFAC Source (downloading CSV)
        builder.Services.AddHttpClient<ISanctionsScreeningService, SanctionsScreeningOfacSourceService>();
        Console.WriteLine("✓ Sanctions Screening: REAL (US Treasury OFAC - Free Source)");
    }
    else
    {
        // Register HttpClient for OpenSanctions API (Default)
        builder.Services.AddHttpClient<ISanctionsScreeningService, SanctionsScreeningOpenSanctionsService>();
        Console.WriteLine("✓ Sanctions Screening: REAL (OpenSanctions.org API - 300+ sources)");
    }
}

Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("");

var app = builder.Build();

// Ensure database is created (for all environments)
// This will create the schema if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Database initialization warning: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRouting();

// Only use authentication if it was configured
if (!string.IsNullOrEmpty(azureAdClientId))
{
    app.UseAuthentication();
}

app.UseAuthorization();
app.MapControllers();

// --- EMAIL SERVICE STARTUP CHECK ---
var config = app.Services.GetRequiredService<IConfiguration>();
useLocalEmulators = config.GetValue<bool>("UseLocalEmulators"); // Re-fetch as it might have been overridden by KeyVault
var smtpEnabled = config.GetValue<bool>("EmailService:Smtp:Enabled", false);
var smtpHost = config["EmailService:Smtp:Host"] ?? config["EmailService__Smtp__Host"];
var smtpUsername = config["EmailService:Smtp:Username"] ?? config["EmailService__Smtp__Username"];
var smtpPassword = config["EmailService:Smtp:Password"] ?? config["EmailService__Smtp__Password"];
var smtpFromEmail = config["EmailService:Smtp:FromEmail"] ?? config["EmailService__Smtp__FromEmail"];

var smtpConfigured = smtpEnabled 
    && !string.IsNullOrEmpty(smtpHost)
    && !string.IsNullOrEmpty(smtpUsername)
    && !string.IsNullOrEmpty(smtpPassword)
    && !string.IsNullOrEmpty(smtpFromEmail);

Console.WriteLine("");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("📧 EMAIL SERVICE CONFIGURATION");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine($"Environment: {(useLocalEmulators ? "Local Development" : "Azure/Production")}");

if (useLocalEmulators)
{
    if (smtpConfigured)
    {
        Console.WriteLine($"✅ SMTP: CONFIGURED");
        Console.WriteLine($"   Host: {smtpHost}");
        Console.WriteLine($"   From: {smtpFromEmail}");
        Console.WriteLine($"   Status: Real emails will be sent via SMTP");
    }
    else
    {
        Console.WriteLine($"⚠️  SMTP: NOT CONFIGURED");
        Console.WriteLine($"   Status: Emails will be LOGGED TO CONSOLE");
        Console.WriteLine($"   To enable real emails, configure SMTP in appsettings.Development.json");
        Console.WriteLine($"   or use 'dotnet user-secrets set' commands");
    }
}
else
{
    var functionUrl = config["EmailService:FunctionUrl"];
    if (!string.IsNullOrEmpty(functionUrl))
    {
        Console.WriteLine($"✅ Azure Communication Services: CONFIGURED");
        Console.WriteLine($"   Function URL: {functionUrl}");
    }
    else if (smtpConfigured)
    {
        Console.WriteLine($"✅ SMTP: CONFIGURED (Fallback)");
        Console.WriteLine($"   From: {smtpFromEmail}");
    }
    else
    {
        Console.WriteLine($"❌ EMAIL SERVICE: NOT CONFIGURED");
        Console.WriteLine($"   WARNING: Email notifications will not be sent!");
    }
}
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("");

app.Run();
