using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VendorMdm.Api.Models;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Models.Sanctions;
using VendorMdm.Api.Services;
using VendorMdm.Api.Tests.Helpers;
using Xunit;
using VendorMdm.Api.Data;

namespace VendorMdm.Api.Tests.Services;

public class VendorServiceTests : TestBase
{
    private (VendorService service, SqlDbContext context, Mock<ISanctionsScreeningService> mockSanctions) CreateService()
    {
        var context = CreateInMemoryDbContext();
        var logger = CreateMockLogger<VendorService>();
        var cosmosClient = MockHelpers.CreateMockCosmosClient();
        // CosmosRepository takes ONLY CosmosClient. Remove configuration.
        var cosmosRepo = new Mock<CosmosRepository>(cosmosClient.Object);
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        
        // Default safe mock
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Clear });

        var service = new VendorService(context, cosmosRepo.Object, logger.Object, mockSanctions.Object);
        return (service, context, mockSanctions);
    }

    [Fact]
    public async Task CreateVendorAsync_ValidVendor_CreatesVendor()
    {
        // Arrange
        var (service, context, _) = CreateService();
        var vendor = new Vendor { LegalName = "New Vendor", PrimaryContactEmail = "test@new.com" };

        // Act
        var result = await service.CreateVendorAsync(vendor);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        context.Vendors.Should().Contain(v => v.Id == result.Id);
    }

    [Fact]
    public async Task CreateVendorAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var (service, context, _) = CreateService();
        context.Vendors.Add(new Vendor { Id = Guid.NewGuid(), PrimaryContactEmail = "dup@test.com", LegalName = "Existing" });
        await context.SaveChangesAsync();
        
        var vendor = new Vendor { LegalName = "New Dup", PrimaryContactEmail = "dup@test.com" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateVendorAsync(vendor));
    }

    [Fact]
    public async Task CreateVendorAsync_HighRiskSanction_ThrowsException()
    {
        // Arrange
        var (service, context, mockSanctions) = CreateService();
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Critical });

        var vendor = new Vendor { LegalName = "Bad Actor", PrimaryContactEmail = "bad@test.com" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateVendorAsync(vendor));
    }

    [Fact]
    public async Task CreateVendorAsync_HighRiskSanction_ForceCreation_CreatesVendor()
    {
        // Arrange
        var (service, context, mockSanctions) = CreateService();
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Critical });

        var vendor = new Vendor { LegalName = "Bad Actor Allowed", PrimaryContactEmail = "bad_allowed@test.com" };

        // Act
        var result = await service.CreateVendorAsync(vendor, forceCreation: true);

        // Assert
        result.Should().NotBeNull();
        context.Vendors.Should().Contain(v => v.Id == result.Id);
    }
}
