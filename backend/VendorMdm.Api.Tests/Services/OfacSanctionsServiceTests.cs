using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using VendorMdm.Api.Services;
using VendorMdm.Api.Services.Helpers;
using VendorMdm.Shared.Models.Sanctions;
using Xunit;

namespace VendorMdm.Api.Tests.Services;

public class OfacSanctionsServiceTests
{
    /// <summary>
    /// Integration test that downloads real OFAC sanctions list (~2MB).
    /// Skip in CI with: dotnet test --filter "Category!=Integration"
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ScreenEntityAsync_RealDownload_FindsPutin()
    {
        // Arrange
        var logger = NullLogger<SanctionsScreeningOfacSourceService>.Instance;
        var httpClient = new HttpClient();
        var matcher = new LevenshteinMatcher();
        
        var configParams = new Dictionary<string, string>
        {
            {"Services:SanctionsScreening:OfacSettings:SourceUrl", "https://sanctionslistservice.ofac.treas.gov/api/publicationpreview/exports/sdn.csv"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();

        var service = new SanctionsScreeningOfacSourceService(logger, httpClient, config, matcher);

        var request = new ScreeningRequest
        {
            VendorId = Guid.NewGuid().ToString(),
            EntityName = "PUTIN, Vladimir Vladimirovich", // Exact match format in OFAC list
            EntityType = "Person"
        };

        // Act
        // This will trigger the download (~2MB), might take a few seconds
        var result = await service.ScreenEntityAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ConfirmedMatch", result.Status);
        Assert.True(result.Matches.Count > 0);
        Assert.Contains(result.Matches, m => m.MatchedName.Contains("PUTIN", StringComparison.OrdinalIgnoreCase));
        
        // Check Metadata
        var updateInfo = await service.GetListsUpdateInfoAsync();
        Assert.True(updateInfo.TotalEntries > 1000); // Should be thousands of entries
    }
}
