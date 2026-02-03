using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VendorMdm.Api.Models;
using VendorMdm.Shared.Models; // SQL entities
using VendorMdm.Api.Services;
using VendorMdm.Api.Tests.Helpers;
using Xunit;

namespace VendorMdm.Api.Tests.Services;

public class InvitationServiceTests : TestBase
{
    [Fact]
    public async Task CreateInvitationAsync_ValidRequest_CreatesInvitation()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var logger = CreateMockStructuredLogger();
        var mockServiceBus = new Mock<IServiceBusService>();
        var mockEmail = new Mock<IEmailService>();
        var mockConfig = MockHelpers.CreateMockConfiguration();
        var mockCosmosClient = MockHelpers.CreateMockCosmosClient();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<VendorMdm.Shared.Models.Sanctions.ScreeningRequest>()))
            .ReturnsAsync(new VendorMdm.Shared.Models.Sanctions.ScreeningResult { OverallRisk = VendorMdm.Shared.Models.Sanctions.RiskLevel.Clear });
        
        var mockAuditLog = new Mock<IAuditLogService>();

        var service = new InvitationService(
            context, logger.Object, mockServiceBus.Object,
            mockEmail.Object, mockConfig, mockSanctions.Object, mockCosmosClient.Object, mockAuditLog.Object);

        var request = new CreateInvitationRequest
        {
            VendorLegalName = "Test Vendor",
            PrimaryContactEmail = "test@vendor.com",
            ExpirationDays = 14,
            Notes = "Test Notes"
        };
        
        // Act
        var result = await service.CreateInvitationAsync(
            request, Guid.NewGuid(), "Test Admin");
        
        // Assert
        result.Should().NotBeNull();
        result.InvitationId.Should().NotBeEmpty();
        
        var invitation = await context.VendorInvitations
            .FirstOrDefaultAsync(i => i.Id == result.InvitationId);
        invitation.Should().NotBeNull();
        invitation.VendorLegalName.Should().Be("Test Vendor");
        invitation.Status.Should().Be(InvitationStatus.Pending);
    }
    
    [Fact]
    public async Task CreateInvitationAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var logger = CreateMockStructuredLogger();
        var mockServiceBus = new Mock<IServiceBusService>();
        var mockEmail = new Mock<IEmailService>();
        var mockConfig = MockHelpers.CreateMockConfiguration();
        var mockCosmosClient = MockHelpers.CreateMockCosmosClient();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        var mockAuditLog = new Mock<IAuditLogService>();

        var service = new InvitationService(
            context, logger.Object, mockServiceBus.Object,
            mockEmail.Object, mockConfig, mockSanctions.Object, mockCosmosClient.Object, mockAuditLog.Object);

        // Pre-seed an invitation
        context.VendorInvitations.Add(new VendorInvitation
        {
            Id = Guid.NewGuid(),
            VendorLegalName = "Existing Vendor",
            PrimaryContactEmail = "duplicate@vendor.com",
            Status = InvitationStatus.Pending
        });
        await context.SaveChangesAsync();
        
        var request = new CreateInvitationRequest
        {
            VendorLegalName = "New Vendor",
            PrimaryContactEmail = "duplicate@vendor.com", // Duplicate
            ExpirationDays = 14
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.CreateInvitationAsync(request, Guid.NewGuid(), "Test Admin"));
    }
    
    [Fact]
    public async Task ValidateInvitationAsync_ValidToken_ReturnsValid()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var logger = CreateMockStructuredLogger();
        var mockServiceBus = new Mock<IServiceBusService>();
        var mockEmail = new Mock<IEmailService>();
        var mockConfig = MockHelpers.CreateMockConfiguration();
        var mockCosmosClient = MockHelpers.CreateMockCosmosClient();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        var mockAuditLog = new Mock<IAuditLogService>();

        var service = new InvitationService(
            context, logger.Object, mockServiceBus.Object,
            mockEmail.Object, mockConfig, mockSanctions.Object, mockCosmosClient.Object, mockAuditLog.Object);

        var token = "valid-token";
        context.VendorInvitations.Add(new VendorInvitation
        {
            Id = Guid.NewGuid(),
            VendorLegalName = "Valid Vendor",
            PrimaryContactEmail = "valid@vendor.com",
            InvitationToken = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync();
        
        // Act
        var result = await service.ValidateInvitationAsync(token);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.VendorLegalName.Should().Be("Valid Vendor");
    }
    
    [Fact]
    public async Task ValidateInvitationAsync_ExpiredToken_ReturnsInvalid()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var logger = CreateMockStructuredLogger();
        var mockServiceBus = new Mock<IServiceBusService>();
        var mockEmail = new Mock<IEmailService>();
        var mockConfig = MockHelpers.CreateMockConfiguration();
        var mockCosmosClient = MockHelpers.CreateMockCosmosClient();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        var mockAuditLog = new Mock<IAuditLogService>();

        var service = new InvitationService(
            context, logger.Object, mockServiceBus.Object,
            mockEmail.Object, mockConfig, mockSanctions.Object, mockCosmosClient.Object, mockAuditLog.Object);

        var token = "expired-token";
        context.VendorInvitations.Add(new VendorInvitation
        {
            Id = Guid.NewGuid(),
            VendorLegalName = "Expired Vendor",
            PrimaryContactEmail = "expired@vendor.com",
            InvitationToken = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();
        
        // Act
        var result = await service.ValidateInvitationAsync(token);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired");
        
        // Verify status updated to Expired
        var invitation = await context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        invitation.Status.Should().Be(InvitationStatus.Expired);
        invitation.Status.Should().Be(InvitationStatus.Expired);
    }

    [Fact]
    public async Task CreateInvitationAsync_WithInternalData_PersistsToAttributes()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var logger = CreateMockStructuredLogger();
        var mockServiceBus = new Mock<IServiceBusService>();
        var mockEmail = new Mock<IEmailService>();
        var mockConfig = MockHelpers.CreateMockConfiguration();
        var mockCosmosClient = MockHelpers.CreateMockCosmosClient();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        mockSanctions.Setup(s => s.ScreenEntityAsync(It.IsAny<VendorMdm.Shared.Models.Sanctions.ScreeningRequest>()))
            .ReturnsAsync(new VendorMdm.Shared.Models.Sanctions.ScreeningResult { OverallRisk = VendorMdm.Shared.Models.Sanctions.RiskLevel.Clear });
        var mockAuditLog = new Mock<IAuditLogService>();

        var service = new InvitationService(
            context, logger.Object, mockServiceBus.Object,
            mockEmail.Object, mockConfig, mockSanctions.Object, mockCosmosClient.Object, mockAuditLog.Object);

        var request = new CreateInvitationRequest
        {
            VendorLegalName = "Internal Vendor",
            PrimaryContactEmail = "internal@vendor.com",
            Currency = "USD",
            SapLanguage = "FR",
            TaxCode1 = "A1",
            TaxCode2 = "B2",
            PermittedPayee = "12345"
        };

        // Act
        var result = await service.CreateInvitationAsync(
            request, Guid.NewGuid(), "Admin");

        // Assert
        var invitation = await context.VendorInvitations
            .FirstOrDefaultAsync(i => i.Id == result.InvitationId);
        
        invitation.Should().NotBeNull();
        invitation.Attributes.Should().NotBeNullOrEmpty();
        invitation.Attributes.Should().Contain("USD");
        invitation.Attributes.Should().Contain("FR");
        invitation.Attributes.Should().Contain("A1");
        invitation.Attributes.Should().Contain("B2");
        invitation.Attributes.Should().Contain("12345");
        invitation.Attributes.Should().Contain("Currency");
        invitation.Attributes.Should().Contain("SapLanguage");
    }

    [Fact]
    public async Task ResendInvitationAsync_ValidRequest_PropagatesEmailSentStatus()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var logger = CreateMockStructuredLogger();
        var mockServiceBus = new Mock<IServiceBusService>();
        var mockEmail = new Mock<IEmailService>();
        var mockConfig = MockHelpers.CreateMockConfiguration();
        var mockCosmosClient = MockHelpers.CreateMockCosmosClient();
        var mockSanctions = new Mock<ISanctionsScreeningService>();
        var mockAuditLog = new Mock<IAuditLogService>();

        var service = new InvitationService(
            context, logger.Object, mockServiceBus.Object,
            mockEmail.Object, mockConfig, mockSanctions.Object, mockCosmosClient.Object, mockAuditLog.Object);

        var invitationId = Guid.NewGuid();
        var originalToken = "original-token";
        context.VendorInvitations.Add(new VendorInvitation
        {
            Id = invitationId,
            VendorLegalName = "Resend Vendor",
            PrimaryContactEmail = "resend@vendor.com",
            InvitationToken = originalToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync();

        // Setup email mock to return false (failure) to test propagation
        mockEmail.Setup(e => e.SendInvitationEmailAsync(It.IsAny<InvitationEmailData>()))
            .ReturnsAsync((false, "Mock Error"));

        // Act
        var result = await service.ResendInvitationAsync(invitationId, Guid.NewGuid());

        // Assert
        result.Success.Should().BeTrue();
        result.EmailSent.Should().BeFalse(); // Propagated from mock
        result.Link.Should().NotBeNullOrEmpty();
        
        var invitation = await context.VendorInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
        invitation.InvitationToken.Should().NotBe(originalToken); // New token generated
    }
}
