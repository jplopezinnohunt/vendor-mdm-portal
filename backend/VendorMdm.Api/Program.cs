using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<ServiceBusService>();
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
app.UseAuthorization();
app.MapControllers();

app.Run();
