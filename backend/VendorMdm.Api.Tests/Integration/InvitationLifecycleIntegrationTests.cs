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

        var createResponse = await service.CreateInvitationAsync(request, Guid.NewGuid(), "Tester");
        var token = createResponse.InvitationToken;

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
        mfaResult.Should().BeTrue();

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
        
        // 7. Complete Invitation (Triggers Application Creation & Screening)
        var completeRequest = new CompleteInvitationRequest
        {
            CompanyName = "Lifecycle Test Vendor",
            ContactName = "Tester",
            Email = "lifecycle@test.com",
            TaxId = "TAX123",
            Attributes = enrichmentData
        };
        
        // We need to call the Controller logic essentially, or the Service method if we had one that bundled it.
        // Since we are testing Service layer, we simulate what the Controller does:
        // 1. Create App
        // 2. Screen
        // 3. Complete Invitation
        
        // WAIT: The Service does NOT have a method that does all this. The Controller coordinates it.
        // To properly test "InvitationLifecycle", we should arguably be testing the SERVICE method `CompleteInvitationAsync`.
        // BUT `CompleteInvitationAsync` in Service ONLY updates the status and emits events. It doesn't create the Application.
        // The Application creation logic is in the CONTROLLER.
        
        // In a proper Clean Architecture, "CreateApplicationFromInvitation" should be a Service method.
        // Refactoring opportunity: Move Controller logic to `InvitationService.CreateApplicationFromInvitationAsync`.
        
        // For now, to verify the FIX works (Frontend calls Complete > Controller Logic > Service Complete),
        // we essentially need to manually do what the Controller does in this test to verify the Service *allows* it.
        
        // But the previous test failed because "SubmitEnrichment" marked it as completed.
        // Now it shouldn't.
        inv5.Status.Should().Be(InvitationStatus.Pending); // Should still be pending/enriched, not Completed
        
        // Simulate Controller Logic:
        var app = new VendorApplication { Id = Guid.NewGuid(), ContactEmail = "lifecycle@test.com", Status="PendingReview" };
        context.VendorApplications.Add(app);
        await context.SaveChangesAsync();
        
        // Now call Service Complete
        var completed = await service.CompleteInvitationAsync(token, app.Id);
        completed.Should().BeTrue();
        
        var invFinal = await context.VendorInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        invFinal.Status.Should().Be(InvitationStatus.Completed);
        invFinal.VendorApplicationId.Should().Be(app.Id);
    }
}
