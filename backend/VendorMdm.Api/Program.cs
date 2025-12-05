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
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Vite default ports
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 1. Azure Clients (Managed Identity)
builder.Services.AddAzureClients(clientBuilder =>
{
    // Service Bus
    clientBuilder.AddServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus"));
    
    // Default Credential (Managed Identity in Azure, VS Creds locally)
    clientBuilder.UseCredential(new DefaultAzureCredential());
});

// 2. SQL Database
builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));

// 3. Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Cosmos");
    return new CosmosClient(connectionString, new DefaultAzureCredential());
});

// 4. Custom Services
builder.Services.AddScoped<CosmosRepository>();
builder.Services.AddScoped<ServiceBusService>();
builder.Services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
