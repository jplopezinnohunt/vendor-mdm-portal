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
using VendorMdm.Core.Framework.Events;
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
        var mockEventDispatcher = new Mock<IDomainEventDispatcher>();

        // Default safe mock
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Clear });

        var service = new VendorService(context, cosmosRepo.Object, logger.Object, mockSanctions.Object, mockEventDispatcher.Object);
        return (service, context, mockSanctions);
    }

    [Fact]
    public async Task CreateVendorAsync_ValidVendor_ReturnsSuccessResult()
    {
        // Arrange
        var (service, context, _) = CreateService();
        var vendor = new Vendor { LegalName = "New Vendor", PrimaryContactEmail = "test@new.com" };

        // Act
        var result = await service.CreateVendorAsync(vendor);

        // Assert - Now using Result pattern
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
        context.Vendors.Should().Contain(v => v.Id == result.Value.Id);
    }

    [Fact]
    public async Task CreateVendorAsync_DuplicateEmail_ReturnsFailureResult()
    {
        // Arrange
        var (service, context, _) = CreateService();
        context.Vendors.Add(new Vendor { Id = Guid.NewGuid(), PrimaryContactEmail = "dup@test.com", LegalName = "Existing" });
        await context.SaveChangesAsync();

        var vendor = new Vendor { LegalName = "New Dup", PrimaryContactEmail = "dup@test.com" };

        // Act
        var result = await service.CreateVendorAsync(vendor);

        // Assert - Now returns Result.Fail instead of throwing
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateVendorAsync_HighRiskSanction_ReturnsFailureResult()
    {
        // Arrange
        var (service, context, mockSanctions) = CreateService();
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Critical });

        var vendor = new Vendor { LegalName = "Bad Actor", PrimaryContactEmail = "bad@test.com" };

        // Act
        var result = await service.CreateVendorAsync(vendor);

        // Assert - Now returns Result.Fail instead of throwing
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Sanctions");
    }

    [Fact]
    public async Task CreateVendorAsync_HighRiskSanction_ForceCreation_ReturnsSuccessResult()
    {
        // Arrange
        var (service, context, mockSanctions) = CreateService();
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Critical });

        var vendor = new Vendor { LegalName = "Bad Actor Allowed", PrimaryContactEmail = "bad_allowed@test.com" };

        // Act
        var result = await service.CreateVendorAsync(vendor, forceCreation: true);

        // Assert - ForceCreation bypasses sanctions check
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        context.Vendors.Should().Contain(v => v.Id == result.Value.Id);
    }
}
