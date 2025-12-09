using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;

namespace VendorMdm.Api.Tests.Helpers;

public static class MockHelpers
{
    public static Mock<CosmosClient> CreateMockCosmosClient()
    {
        var mockClient = new Mock<CosmosClient>();
        var mockContainer = new Mock<Container>();
        
        // Mock GetContainer to return a mock container
        mockClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockContainer.Object);
            
        // Mock upsert item to return a valid response
        mockContainer.Setup(c => c.UpsertItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ItemResponse<object>>().Object);
            
        // Mock create item
        mockContainer.Setup(c => c.CreateItemAsync(It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
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
