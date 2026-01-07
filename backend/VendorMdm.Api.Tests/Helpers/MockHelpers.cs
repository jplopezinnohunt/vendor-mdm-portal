using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;

namespace VendorMdm.Api.Tests.Helpers;

public static class MockHelpers
{
    public static Mock<CosmosClient> CreateMockCosmosClient()
    {
        var mockClient = new Mock<CosmosClient>();
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();
        
        // Mock GetDatabase
        mockClient.Setup(c => c.GetDatabase(It.IsAny<string>()))
            .Returns(mockDatabase.Object);

        // Mock GetContainer (Direct from Client - commonly used)
        mockClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockContainer.Object);
            
        // Mock GetContainer (From Database - used in Repository)
        mockDatabase.Setup(d => d.GetContainer(It.IsAny<string>()))
            .Returns(mockContainer.Object);
            
        // Mock upsert item to return a valid response
        mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ItemResponse<object>>().Object);
            
        // Mock create item
        mockContainer.Setup(c => c.CreateItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ItemResponse<object>>().Object);
            
        // Mock ReadItemAsync (Generic)
        mockContainer.Setup(c => c.ReadItemAsync<object>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new Mock<ItemResponse<object>>().Object);
            
        return mockClient;
    }
    
    public static IConfiguration CreateMockConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"App:BaseUrl", "http://localhost:3000"},
            {"App:CompanyName", "Test Company"},
            {"UseLocalEmulators", "false"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }
}
