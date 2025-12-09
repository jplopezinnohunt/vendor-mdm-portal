using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrApprover", policy =>
        policy.RequireRole("Admin", "Approver"));
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:3002") // Vite default ports + alternative
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// --- CONNECTION STRING LOGIC ---
var useLocalEmulators = builder.Configuration.GetValue<bool>("UseLocalEmulators");
var sqlConnection = builder.Configuration.GetConnectionString("Sql");
var cosmosConnection = builder.Configuration.GetConnectionString("Cosmos");
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");

// Auto-fallback: If Azure connection strings are still placeholders, use local emulators
if (!useLocalEmulators && (sqlConnection?.Contains("YOUR_") == true || 
    sqlConnection?.Contains("YOUR_SERVER_NAME") == true || 
    cosmosConnection?.Contains("YOUR_") == true ||
    serviceBusConnection?.Contains("YOUR_") == true ||
    string.IsNullOrEmpty(sqlConnection)))
{
    Console.WriteLine("⚠️ Azure connection strings contain placeholders. Falling back to Local Emulators.");
    Console.WriteLine("💡 To use Azure resources, update appsettings.Development.json or use User Secrets.");
    useLocalEmulators = true;
}

if (useLocalEmulators)
{
    Console.WriteLine("🔧 Using Local Emulators for development.");
    sqlConnection = builder.Configuration.GetSection("LocalConnectionStrings")["Sql"];
    cosmosConnection = builder.Configuration.GetSection("LocalConnectionStrings")["Cosmos"];
    serviceBusConnection = builder.Configuration.GetSection("LocalConnectionStrings")["ServiceBus"];
}
else
{
    Console.WriteLine("☁️ Using Azure Resources.");
}

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

var app = builder.Build();

// Ensure database is created (for local development)
if (useLocalEmulators)
{
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
app.UseAuthentication();
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
