using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;
using VendorMdm.Api.Tests.Helpers;
using VendorMdm.Core.Framework.Primitives;
using VendorMdm.Shared.Constants;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Models.Sanctions;
using VendorMdm.Shared.Models.SapSimulation;
using Xunit;

namespace VendorMdm.Api.Tests.Services;

public class VendorApplicationServiceTests : TestBase
{
    private (VendorApplicationService service, SqlDbContext context, Mock<IVendorService> mockVendorService, Mock<ISanctionsScreeningService> mockSanctions, Mock<ISapVendorService> mockSap, Mock<IServiceBusService> mockServiceBus) CreateService()
    {
        var context = CreateInMemoryDbContext();
        var logger = CreateMockLogger<VendorApplicationService>();

        var mockVendorService = new Mock<IVendorService>();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        var mockSap = new Mock<ISapVendorService>();
        var mockServiceBus = new Mock<IServiceBusService>();

        // Default safe mocks
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Clear });

        mockSap.Setup(s => s.CreateVendorAsync(It.IsAny<VendorCreateRequest>()))
            .ReturnsAsync(new VendorCreateResponse { VendorNumber = "SAP001", Success = true });

        mockServiceBus.Setup(s => s.PublishEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var service = new VendorApplicationService(
            context,
            mockVendorService.Object,
            mockSanctions.Object,
            mockSap.Object,
            mockServiceBus.Object,
            logger.Object);

        return (service, context, mockVendorService, mockSanctions, mockSap, mockServiceBus);
    }

    private async Task<VendorApplication> CreateTestApplication(SqlDbContext context, string status = ApplicationStatus.PendingReview)
    {
        var app = new VendorApplication
        {
            Id = Guid.NewGuid(),
            CompanyName = "Test Company",
            TaxId = "123456789",
            ContactName = "John Doe",
            ContactEmail = "john@test.com",
            Status = status,
            Attributes = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "street1", "123 Main St" },
                { "city", "New York" },
                { "country", "US" }
            }),
            CreatedAt = DateTime.UtcNow
        };
        context.VendorApplications.Add(app);
        await context.SaveChangesAsync();
        return app;
    }

    #region GetApplicationAsync Tests

    [Fact]
    public async Task GetApplicationAsync_ExistingApplication_ReturnsApplication()
    {
        // Arrange
        var (service, context, _, _, _, _) = CreateService();
        var app = await CreateTestApplication(context);

        // Act
        var result = await service.GetApplicationAsync(app.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(app.Id);
        result.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task GetApplicationAsync_NonExistingApplication_ReturnsNull()
    {
        // Arrange
        var (service, _, _, _, _, _) = CreateService();

        // Act
        var result = await service.GetApplicationAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ApproveApplicationAsync Tests

    [Fact]
    public async Task ApproveApplicationAsync_ValidApplication_ReturnsSuccessAndCreatesVendor()
    {
        // Arrange
        var (service, context, mockVendorService, _, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        var createdVendor = new Vendor
        {
            Id = Guid.NewGuid(),
            LegalName = "Test Company",
            PrimaryContactEmail = "john@test.com"
        };

        mockVendorService.Setup(v => v.CreateVendorAsync(It.IsAny<Vendor>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(createdVendor));

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.LegalName.Should().Be("Test Company");

        // Verify application status changed
        var updatedApp = await context.VendorApplications.FindAsync(app.Id);
        updatedApp!.Status.Should().Be(ApplicationStatus.Approved);
    }

    [Fact]
    public async Task ApproveApplicationAsync_WithEnrichedAttributes_MergesAttributes()
    {
        // Arrange
        var (service, context, mockVendorService, _, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        var createdVendor = new Vendor
        {
            Id = Guid.NewGuid(),
            LegalName = "Updated Company Name",
            PrimaryContactEmail = "john@test.com"
        };

        mockVendorService.Setup(v => v.CreateVendorAsync(It.IsAny<Vendor>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(createdVendor));

        var enrichedAttributes = new Dictionary<string, object>
        {
            { "companyName", "Updated Company Name" },
            { "customField", "CustomValue" }
        };

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, enrichedAttributes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify application was updated with enriched data
        var updatedApp = await context.VendorApplications.FindAsync(app.Id);
        updatedApp!.CompanyName.Should().Be("Updated Company Name");
        updatedApp.Attributes.Should().Contain("customField");
    }

    [Fact]
    public async Task ApproveApplicationAsync_NonExistingApplication_ReturnsFailure()
    {
        // Arrange
        var (service, _, _, _, _, _) = CreateService();

        // Act
        var result = await service.ApproveApplicationAsync(Guid.NewGuid(), null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ApproveApplicationAsync_InvalidStateTransition_ReturnsFailure()
    {
        // Arrange
        var (service, context, _, _, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.Approved); // Already approved

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot approve");
    }

    [Fact]
    public async Task ApproveApplicationAsync_HighRiskSanctions_ReturnsFailure()
    {
        // Arrange
        var (service, context, mockVendorService, mockSanctions, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Critical });

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null, forceSanctionsOverride: false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Sanctions");
    }

    [Fact]
    public async Task ApproveApplicationAsync_HighRiskSanctionsWithOverride_ReturnsSuccess()
    {
        // Arrange
        var (service, context, mockVendorService, mockSanctions, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        var createdVendor = new Vendor
        {
            Id = Guid.NewGuid(),
            LegalName = "Test Company",
            PrimaryContactEmail = "john@test.com"
        };

        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<ScreeningRequest>()))
            .ReturnsAsync(new ScreeningResult { OverallRisk = RiskLevel.Critical });

        mockVendorService.Setup(v => v.CreateVendorAsync(It.IsAny<Vendor>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(createdVendor));

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null, forceSanctionsOverride: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveApplicationAsync_SapIntegrationFails_ReturnsFailure()
    {
        // Arrange
        var (service, context, mockVendorService, _, mockSap, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        var createdVendor = new Vendor
        {
            Id = Guid.NewGuid(),
            LegalName = "Test Company",
            PrimaryContactEmail = "john@test.com"
        };

        mockVendorService.Setup(v => v.CreateVendorAsync(It.IsAny<Vendor>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(createdVendor));

        mockSap.Setup(s => s.CreateVendorAsync(It.IsAny<VendorCreateRequest>()))
            .ThrowsAsync(new Exception("SAP Connection Failed"));

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SAP");
    }

    [Fact]
    public async Task ApproveApplicationAsync_VendorCreationFails_ReturnsFailure()
    {
        // Arrange
        var (service, context, mockVendorService, _, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        mockVendorService.Setup(v => v.CreateVendorAsync(It.IsAny<Vendor>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Fail<Vendor>("Duplicate vendor"));

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Vendor creation failed");
    }

    [Fact]
    public async Task ApproveApplicationAsync_PublishesDomainEvent()
    {
        // Arrange
        var (service, context, mockVendorService, _, _, mockServiceBus) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        var createdVendor = new Vendor
        {
            Id = Guid.NewGuid(),
            LegalName = "Test Company",
            PrimaryContactEmail = "john@test.com"
        };

        mockVendorService.Setup(v => v.CreateVendorAsync(It.IsAny<Vendor>(), It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(createdVendor));

        // Act
        var result = await service.ApproveApplicationAsync(app.Id, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockServiceBus.Verify(
            s => s.PublishEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    #endregion

    #region RejectApplicationAsync Tests

    [Fact]
    public async Task RejectApplicationAsync_ValidApplication_ReturnsSuccess()
    {
        // Arrange
        var (service, context, _, _, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        // Act
        var result = await service.RejectApplicationAsync(app.Id, "Not qualified");

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedApp = await context.VendorApplications.FindAsync(app.Id);
        updatedApp!.Status.Should().Be(ApplicationStatus.Rejected);
        updatedApp.Attributes.Should().Contain("rejectionReason");
        updatedApp.Attributes.Should().Contain("Not qualified");
    }

    [Fact]
    public async Task RejectApplicationAsync_NonExistingApplication_ReturnsFailure()
    {
        // Arrange
        var (service, _, _, _, _, _) = CreateService();

        // Act
        var result = await service.RejectApplicationAsync(Guid.NewGuid(), "Any reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task RejectApplicationAsync_InvalidStateTransition_ReturnsFailure()
    {
        // Arrange
        var (service, context, _, _, _, _) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.Approved); // Already approved

        // Act
        var result = await service.RejectApplicationAsync(app.Id, "Too late");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot reject");
    }

    [Fact]
    public async Task RejectApplicationAsync_PublishesDomainEvent()
    {
        // Arrange
        var (service, context, _, _, _, mockServiceBus) = CreateService();
        var app = await CreateTestApplication(context, ApplicationStatus.PendingReview);

        // Act
        var result = await service.RejectApplicationAsync(app.Id, "Not qualified");

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockServiceBus.Verify(
            s => s.PublishEventAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    #endregion
}
