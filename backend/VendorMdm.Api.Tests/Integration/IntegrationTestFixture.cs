using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VendorMdm.Api.Data;
using VendorMdm.Api.Models;
using VendorMdm.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Azure.Cosmos;
using VendorMdm.Api.Tests.Helpers;
using VendorMdm.Shared.Models.Sanctions;

namespace VendorMdm.Api.Tests.Integration;

public class IntegrationTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }
    public IConfiguration Configuration { get; private set; }
    
    // Mocks for external dependencies in integration tests where real infra is not available
    public Mock<IServiceBusService> MockServiceBus { get; private set; }
    public Mock<IEmailService> MockEmailService { get; private set; }
    public Mock<CosmosClient> MockCosmosClient { get; private set; }

    public IntegrationTestFixture()
    {
        var services = new ServiceCollection();
        
        // Configuration
        Configuration = MockHelpers.CreateMockConfiguration();
        services.AddSingleton(Configuration);
        
        // DB Setup (InMemory for "Integration" simulation if real DB not present)
        // In a true integration test, we would use TestContainers or real connection string
        services.AddDbContext<SqlDbContext>(options => 
            options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid()));
            
        // Service Bus (Mocked for safety/cost)
        MockServiceBus = new Mock<IServiceBusService>();
        services.AddSingleton(MockServiceBus.Object);
        
        // Email (Mocked)
        MockEmailService = new Mock<IEmailService>();
        services.AddSingleton(MockEmailService.Object);
        
        // Cosmos (Mocked for now, as Emulator setup is complex in this env)
        // Ideally use TestContainers for Cosmos
        MockCosmosClient = MockHelpers.CreateMockCosmosClient();
        services.AddSingleton(MockCosmosClient.Object);
        
        // Mock Sanctions Service
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        mockSanctions
            .Setup(s => s.ScreenEntityAsync(It.IsAny<VendorMdm.Shared.Models.Sanctions.ScreeningRequest>()))
            .ReturnsAsync(new VendorMdm.Shared.Models.Sanctions.ScreeningResult { OverallRisk = VendorMdm.Shared.Models.Sanctions.RiskLevel.Clear });
        services.AddSingleton(mockSanctions.Object);

        // Repositories & Services
        services.AddScoped<CosmosRepository>();
        services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddLogging();
        
        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
