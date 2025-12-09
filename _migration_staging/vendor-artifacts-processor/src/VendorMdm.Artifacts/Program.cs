using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using VendorMdm.Artifacts.Data;
using VendorMdm.Artifacts.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // 1. SQL Database
        services.AddDbContext<ArtifactDbContext>(options =>
            options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString")));

        // 2. Cosmos DB
        services.AddSingleton<CosmosClient>(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
            // Use DefaultAzureCredential for production, ConnectionString for local if needed
            // For simplicity in this artifact, we assume connection string or managed identity logic here
            return new CosmosClient(connectionString); 
        });

        // 3. Service Bus
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnection"));
        });

        // 4. Domain Services
        services.AddScoped<IArtifactService, ArtifactService>();
        services.AddScoped<IMetadataService, MetadataService>();
    })
    .Build();

host.Run();
