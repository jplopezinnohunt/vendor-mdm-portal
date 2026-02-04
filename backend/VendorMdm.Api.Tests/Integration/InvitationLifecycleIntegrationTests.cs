using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VendorMdm.Api.Data;
using VendorMdm.Api.Models;
using VendorMdm.Api.Services;
using VendorMdm.Shared.Models;
using Xunit;

namespace VendorMdm.Api.Tests.Integration;

public class InvitationLifecycleIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public InvitationLifecycleIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Invitation_FullLifecycle_Verification()
    {
        // Setup Scope
        using var scope = _fixture.ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInvitationService>();
        var context = scope.ServiceProvider.GetRequiredService<SqlDbContext>();

        // 1. Create Invitation
        var request = new CreateInvitationRequest
        {
            VendorLegalName = "Lifecycle Test Vendor",
            PrimaryContactEmail = "lifecycle@test.com",
            VendorType = "Company",
            ExpirationDays = 14
        };

        var createResult = await service.CreateInvitationAsync(request, Guid.NewGuid(), "Tester");
        createResult.IsSuccess.Should().BeTrue();
        var token = createResult.Value.InvitationToken;

        // Verify Status
        var inv1 = await context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        inv1.Status.Should().Be(InvitationStatus.Pending);
        inv1.CurrentStage.Should().Be(InvitationStage.InvitationSent);

        // 2. Validate Token
        var validation = await service.ValidateInvitationAsync(token);
        validation.IsValid.Should().BeTrue();

        // 3. Trigger MFA
        await service.TriggerMfaAsync(token);
        
        // Retrieve code from DB (simulating checking email)
        var inv2 = await context.VendorInvitations.AsNoTracking().FirstOrDefaultAsync(i => i.InvitationToken == token);
        var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(inv2.Attributes);
        var mfaCode = attributes["mfaCode"].ToString();

        // 4. Verify MFA
        var mfaResult = await service.VerifyMfaCodeAsync(token, mfaCode);
        mfaResult.Success.Should().BeTrue();

        var inv3 = await context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        inv3.CurrentStage.Should().Be(InvitationStage.MfaVerified);

        // 5. Submit Initial Info
        var initialInfo = new Dictionary<string, object>
        {
            { "confirmedName", "Lifecycle Test Vendor Inc." }
        };
        await service.SubmitInitialInfoAsync(token, initialInfo);
        
        var inv4 = await context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        inv4.CurrentStage.Should().Be(InvitationStage.InitialInfoCompleted);

        // 6. Submit Enrichment (The Critical Step)
        var enrichmentData = new Dictionary<string, object>
        {
            { "address", "123 Test St" },
            { "bank", "Test Bank" }
        };
        await service.SubmitEnrichmentAsync(token, enrichmentData);

        var inv5 = await context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        inv5.CurrentStage.Should().Be(InvitationStage.Enriched);
        
        // 7. Verify Atomic Completion (New System Behavior)
        // SubmitEnrichmentAsync now atomically creates the application and completes the invitation.
        inv5.Status.Should().Be(InvitationStatus.Completed);
        inv5.VendorApplicationId.Should().NotBeNull();
        
        var app = await context.VendorApplications.FindAsync(inv5.VendorApplicationId);
        app.Should().NotBeNull();
        app.ContactEmail.Should().Be("lifecycle@test.com");
        app.Status.Should().Be("PendingReview");
    }
}
