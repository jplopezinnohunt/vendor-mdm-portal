# Invitation Flow - Implementation Plan

**Created:** 2025-12-08  
**Status:** Not Started  
**Target Completion:** TBD

---

## Overview

This plan addresses the 6 gaps identified in the [Invitation Flow Review](file:///Users/jplopez/projects/vendor-mdm-portal/reviews/invitation-flow-review.md) to bring the flow from 95% to 100% production-ready.

**Total Estimated Effort:** 19-26 hours

---

## 🎯 Phase 1: Critical Fixes (Do First)

### Task 1.1: Add Project Reference & Remove Duplicates ⚡ CRITICAL

**Goal:** Fix broken project references and eliminate code duplication  
**Complexity:** S (Small)  
**Estimated Time:** 1-2 hours  
**Priority:** 🔴 HIGHEST

#### Steps:

1. **Add Project Reference to VendorMdm.Api**
   ```bash
   # Navigate to Api project
   cd backend/VendorMdm.Api
   
   # Add reference to Shared project
   dotnet add reference ../VendorMdm.Shared/VendorMdm.Shared.csproj
   ```

2. **Update Using Statements in Service Files**
   
   Files to update:
   - [InvitationService.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Services/InvitationService.cs)
   - [ChangeRequestRepository.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Services/ChangeRequestRepository.cs)
   - Any other service files
   
   Change:
   ```csharp
   // FROM:
   using VendorMdm.Api.Models;
   
   // TO:
   using VendorMdm.Shared.Models;
   using VendorMdm.Api.Models; // Keep for DTOs
   ```

3. **Remove Duplicate Entity Models**
   
   Delete or comment out duplicate classes in:
   - [VendorMdm.Api/Models/SqlEntities.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Models/SqlEntities.cs)
   
   Keep only:
   - DTOs (CreateInvitationRequest, etc.)
   - Cosmos entities (InvitationArtifact, etc.)
   
   Remove:
   - VendorInvitation (use from Shared)
   - VendorApplication (use from Shared)
   - ChangeRequest (use from Shared)
   - Other duplicates

4. **Update DbContext**
   
   File: [SqlDbContext.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Data/SqlDbContext.cs)
   
   Update using:
   ```csharp
   using VendorMdm.Shared.Models;
   ```

5. **Build and Verify**
   ```bash
   cd backend/VendorMdm.Api
   dotnet build
   
   # Should build without errors
   ```

6. **Run and Test**
   ```bash
   dotnet run --project backend/VendorMdm.Api
   
   # Test: Create invitation via Swagger or API call
   # Verify: Database tables still work correctly
   ```

#### Acceptance Criteria:
- ✅ VendorMdm.Api.csproj contains `<ProjectReference>` to VendorMdm.Shared
- ✅ No duplicate entity classes in VendorMdm.Api/Models/SqlEntities.cs
- ✅ `dotnet build` succeeds
- ✅ Invitation creation still works via API

---

### Task 1.2: Create Solution File ⚡ QUICK WIN

**Goal:** Improve developer experience and build management  
**Complexity:** XS (Extra Small)  
**Estimated Time:** 15 minutes  
**Priority:** 🟡 MEDIUM

#### Steps:

1. **Create Solution File**
   ```bash
   cd backend
   dotnet new sln -n VendorMdm
   ```

2. **Add All Projects to Solution**
   ```bash
   dotnet sln add VendorMdm.Api/VendorMdm.Api.csproj
   dotnet sln add VendorMdm.Artifacts/VendorMdm.Artifacts.csproj
   dotnet sln add VendorMdm.Shared/VendorMdm.Shared.csproj
   ```

3. **Verify Solution**
   ```bash
   dotnet build VendorMdm.sln
   
   # Should build all 3 projects successfully
   ```

4. **Update README** (optional but recommended)
   
   Add to [README.md](file:///Users/jplopez/projects/vendor-mdm-portal/README.md):
   ```markdown
   ## Building the Backend
   
   ```bash
   cd backend
   dotnet build VendorMdm.sln
   ```
   ```

#### Acceptance Criteria:
- ✅ `backend/VendorMdm.sln` exists
- ✅ Solution contains all 3 projects
- ✅ `dotnet build VendorMdm.sln` succeeds

---

## 🎯 Phase 2: Testing Infrastructure

### Task 2.1: Create Test Project Structure

**Goal:** Establish testing foundation  
**Complexity:** M (Medium)  
**Estimated Time:** 2-3 hours  
**Priority:** 🔴 HIGH

#### Steps:

1. **Create Test Project**
   ```bash
   cd backend
   dotnet new xunit -n VendorMdm.Api.Tests
   cd VendorMdm.Api.Tests
   ```

2. **Add Required Packages**
   ```bash
   # Testing framework
   dotnet add package xUnit --version 2.6.0
   dotnet add package xunit.runner.visualstudio --version 2.5.0
   
   # Mocking
   dotnet add package Moq --version 4.20.0
   
   # In-memory database for testing
   dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
   
   # Test utilities
   dotnet add package FluentAssertions --version 6.12.0
   ```

3. **Add Project References**
   ```bash
   dotnet add reference ../VendorMdm.Api/VendorMdm.Api.csproj
   dotnet add reference ../VendorMdm.Shared/VendorMdm.Shared.csproj
   ```

4. **Create Test Directory Structure**
   ```bash
   mkdir -p Services
   mkdir -p Controllers
   mkdir -p Integration
   mkdir -p Helpers
   ```

5. **Create Base Test Class**
   
   File: `Helpers/TestBase.cs`
   ```csharp
   using Microsoft.EntityFrameworkCore;
   using Microsoft.Extensions.Logging;
   using Moq;
   using VendorMdm.Api.Data;
   
   namespace VendorMdm.Api.Tests.Helpers;
   
   public class TestBase
   {
       protected SqlDbContext CreateInMemoryDbContext()
       {
           var options = new DbContextOptionsBuilder<SqlDbContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options;
           
           return new SqlDbContext(options);
       }
       
       protected Mock<ILogger<T>> CreateMockLogger<T>()
       {
           return new Mock<ILogger<T>>();
       }
   }
   ```

6. **Add to Solution**
   ```bash
   cd ..
   dotnet sln add VendorMdm.Api.Tests/VendorMdm.Api.Tests.csproj
   ```

7. **Verify Setup**
   ```bash
   dotnet test VendorMdm.Api.Tests
   
   # Should run successfully (even if no tests yet)
   ```

#### Acceptance Criteria:
- ✅ `VendorMdm.Api.Tests` project exists
- ✅ All required NuGet packages installed
- ✅ Directory structure created
- ✅ TestBase helper class created
- ✅ `dotnet test` runs successfully

---

### Task 2.2: Write Invitation Service Unit Tests

**Goal:** Test service methods in isolation  
**Complexity:** L (Large)  
**Estimated Time:** 4-6 hours  
**Priority:** 🔴 HIGH

#### Steps:

1. **Create Test File**
   
   File: `Services/InvitationServiceTests.cs`

2. **Implement Test Cases**
   
   Required tests:
   
   ```csharp
   public class InvitationServiceTests : TestBase
   {
       [Fact]
       public async Task CreateInvitationAsync_ValidRequest_CreatesInvitation()
       {
           // Arrange
           var context = CreateInMemoryDbContext();
           var logger = CreateMockLogger<InvitationService>();
           var mockServiceBus = new Mock<ServiceBusService>();
           var mockEmail = new Mock<IEmailService>();
           var mockConfig = CreateMockConfiguration();
           var mockCosmosClient = CreateMockCosmosClient();
           
           var service = new InvitationService(
               context, logger.Object, mockServiceBus.Object, 
               mockEmail.Object, mockConfig, mockCosmosClient);
           
           var request = new CreateInvitationRequest
           {
               VendorLegalName = "Test Vendor",
               PrimaryContactEmail = "test@vendor.com",
               ExpirationDays = 14
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
       }
       
       [Fact]
       public async Task CreateInvitationAsync_DuplicateEmail_ThrowsException()
       {
           // Arrange - create existing invitation
           // Act - try to create duplicate
           // Assert - should throw InvalidOperationException
       }
       
       [Fact]
       public async Task ValidateInvitationAsync_ValidToken_ReturnsValid()
       {
           // Test valid token validation
       }
       
       [Fact]
       public async Task ValidateInvitationAsync_ExpiredToken_ReturnsInvalid()
       {
           // Test expired token validation
       }
       
       [Fact]
       public async Task CompleteInvitationAsync_ValidToken_UpdatesStatus()
       {
           // Test invitation completion
       }
       
       [Fact]
       public async Task ResendInvitationAsync_ValidInvitation_GeneratesNewToken()
       {
           // Test resend functionality
       }
   }
   ```

3. **Create Mock Helpers**
   
   File: `Helpers/MockHelpers.cs`
   ```csharp
   public static class MockHelpers
   {
       public static Mock<CosmosClient> CreateMockCosmosClient()
       {
           // Mock Cosmos client for testing
       }
       
       public static IConfiguration CreateMockConfiguration()
       {
           // Mock configuration for testing
       }
   }
   ```

4. **Run Tests**
   ```bash
   dotnet test --filter "FullyQualifiedName~InvitationServiceTests"
   ```

#### Acceptance Criteria:
- ✅ All 6+ test cases implemented
- ✅ Tests use in-memory database
- ✅ Cosmos and Service Bus are mocked
- ✅ All tests pass
- ✅ Code coverage > 80% for InvitationService

---

### Task 2.3: Write Integration Tests for A→B→C→D Flow

**Goal:** Verify hybrid pattern compliance end-to-end  
**Complexity:** L (Large)  
**Estimated Time:** 6-8 hours  
**Priority:** 🔴 HIGH

#### Steps:

1. **Create Integration Test File**
   
   File: `Integration/InvitationFlowIntegrationTests.cs`

2. **Implement Hybrid Pattern Verification Test**
   
   ```csharp
   [Collection("Integration")]
   public class InvitationFlowIntegrationTests : IAsyncLifetime
   {
       private SqlDbContext _context;
       private Container _artifactsContainer;
       private Container _eventsContainer;
       private InvitationService _service;
       
       public async Task InitializeAsync()
       {
           // Setup: Real SQL context, Cosmos emulator containers
       }
       
       [Fact]
       public async Task CreateInvitation_VerifiesHybridPattern_ABCD()
       {
           // Arrange
           var request = new CreateInvitationRequest
           {
               VendorLegalName = "Integration Test Vendor",
               PrimaryContactEmail = "integration@test.com",
               ExpirationDays = 14
           };
           
           // Act
           var result = await _service.CreateInvitationAsync(
               request, Guid.NewGuid(), "Integration Test");
           
           // Assert A: SQL Database
           var sqlInvitation = await _context.VendorInvitations
               .FirstOrDefaultAsync(i => i.Id == result.InvitationId);
           sqlInvitation.Should().NotBeNull();
           sqlInvitation.Status.Should().Be(InvitationStatus.Pending);
           
           // Assert B: Cosmos Artifacts
           var artifact = await _artifactsContainer
               .ReadItemAsync<InvitationArtifact>(
                   result.InvitationId.ToString(),
                   new PartitionKey(result.InvitationId.ToString()));
           artifact.Resource.Should().NotBeNull();
           artifact.Resource.VendorLegalName.Should().Be("Integration Test Vendor");
           
           // Assert C: Cosmos Events
           var queryDef = new QueryDefinition(
               "SELECT * FROM c WHERE c.eventType = @eventType AND c.entityId = @entityId")
               .WithParameter("@eventType", "InvitationCreated")
               .WithParameter("@entityId", result.InvitationId.ToString());
           
           var events = await _eventsContainer
               .GetItemQueryIterator<DomainEvent>(queryDef)
               .ReadNextAsync();
           events.Should().NotBeEmpty();
           events.First().EventType.Should().Be("InvitationCreated");
           
           // Assert D: Service Bus (verify message published)
           // Note: May need to mock or use test queue
       }
       
       public async Task DisposeAsync()
       {
           // Cleanup
       }
   }
   ```

3. **Setup Integration Test Configuration**
   
   File: `Integration/IntegrationTestFixture.cs` - Setup shared resources

4. **Run Integration Tests**
   ```bash
   dotnet test --filter "Category=Integration"
   ```

#### Acceptance Criteria:
- ✅ Integration test verifies A→B→C→D pattern
- ✅ Test uses real Cosmos emulator (or testcontainers)
- ✅ All assertions pass
- ✅ Test can run in CI environment

---

## 🎯 Phase 3: Security & Production Readiness

### Task 3.1: Implement Authentication/Authorization

**Goal:** Secure invitation endpoints with role-based access  
**Complexity:** M (Medium)  
**Estimated Time:** 3-4 hours  
**Priority:** 🔴 HIGH

#### Steps:

1. **Add Authentication Packages**
   ```bash
   cd backend/VendorMdm.Api
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package Microsoft.Identity.Web
   ```

2. **Configure Authentication in Program.cs**
   
   File: [Program.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Program.cs)
   
   Add before `var app = builder.Build();`:
   ```csharp
   // Authentication & Authorization
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
   
   builder.Services.AddAuthorization(options =>
   {
       options.AddPolicy("AdminOrApprover", policy =>
           policy.RequireRole("Admin", "Approver"));
   });
   ```
   
   Add after `app.UseRouting();`:
   ```csharp
   app.UseAuthentication();
   app.UseAuthorization();
   ```

3. **Add Azure AD Configuration**
   
   File: [appsettings.json](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/appsettings.json)
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "yourdomain.onmicrosoft.com",
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id"
     }
   }
   ```

4. **Update InvitationController with Authorization**
   
   File: [InvitationController.cs](file:///Users/jplopez/projects/vendor-mdm-portal/backend/VendorMdm.Api/Controllers/InvitationController.cs)
   
   ```csharp
   using Microsoft.AspNetCore.Authorization;
   
   [ApiController]
   [Route("api/[controller]")]
   public class InvitationController : ControllerBase
   {
       // Protected endpoints
       [Authorize(Policy = "AdminOrApprover")]
       [HttpPost("create")]
       public async Task<IActionResult> CreateInvitation(...)
       
       [Authorize(Policy = "AdminOrApprover")]
       [HttpGet("list")]
       public async Task<IActionResult> GetInvitations(...)
       
       [Authorize(Policy = "AdminOrApprover")]
       [HttpPost("resend/{id}")]
       public async Task<IActionResult> ResendInvitation(...)
       
       // Public endpoints
       [AllowAnonymous]
       [HttpGet("validate/{token}")]
       public async Task<IActionResult> ValidateInvitation(...)
       
       [AllowAnonymous]
       [HttpGet("details/{token}")]
       public async Task<IActionResult> GetInvitationDetails(...)
       
       [AllowAnonymous]
       [HttpPost("complete/{token}")]
       public async Task<IActionResult> CompleteInvitation(...)
   }
   ```

5. **Get Authenticated User Information**
   
   Replace mock code (lines 35-38):
   ```csharp
   // FROM:
   var invitedBy = Guid.NewGuid();
   var invitedByName = "System Admin";
   
   // TO:
   var invitedBy = Guid.Parse(User.FindFirst("sub")?.Value 
       ?? throw new UnauthorizedAccessException());
   var invitedByName = User.Identity?.Name 
       ?? throw new UnauthorizedAccessException();
   ```

6. **Test Authentication**
   
   Manual testing with Postman:
   - GET `/api/invitation/create` without token → 401 Unauthorized
   - GET `/api/invitation/validate/token123` without token → 200 OK (public)
   - GET `/api/invitation/create` with valid Admin token → 200 OK

#### Acceptance Criteria:
- ✅ Authentication middleware configured
- ✅ Protected endpoints return 401 without token
- ✅ Protected endpoints work with valid token
- ✅ Public endpoints work without token
- ✅ User information extracted from claims

---

### Task 3.2: Add Test Execution to CI

**Goal:** Prevent untested code from deploying  
**Complexity:** S (Small)  
**Estimated Time:** 1 hour  
**Priority:** 🟡 MEDIUM

#### Steps:

1. **Update GitHub Actions Workflow**
   
   File: [.github/workflows/azure-functions.yml](file:///Users/jplopez/projects/vendor-mdm-portal/.github/workflows/azure-functions.yml)
   
   Add after `Setup DotNet` step (around line 40):
   ```yaml
   - name: 'Run Unit Tests'
     shell: pwsh
     run: |
       cd backend
       dotnet test VendorMdm.Api.Tests --configuration Release --logger trx --results-directory ./TestResults
       
   - name: 'Publish Test Results'
     uses: actions/upload-artifact@v3
     if: always()
     with:
       name: test-results
       path: backend/TestResults/*.trx
   ```

2. **Update Workflow to Fail on Test Failure**
   
   The workflow should automatically fail if tests fail.

3. **Test the Workflow**
   
   - Create a failing test
   - Push to main
   - Verify CI fails and doesn't deploy
   - Fix test
   - Push again
   - Verify CI passes and deploys

#### Acceptance Criteria:
- ✅ Tests run in CI before deployment
- ✅ Failed tests block deployment
- ✅ Test results uploaded as artifacts
- ✅ Test run time visible in workflow logs

---

## 🎯 Phase 4: Documentation

### Task 4.1: Create Invitation Flow Documentation

**Goal:** Document end-to-end flow for developers  
**Complexity:** S (Small)  
**Estimated Time:** 2 hours  
**Priority:** 🟢 LOW

#### Steps:

1. **Create Documentation File**
   
   File: `docs/features/invitation-flow.md`

2. **Document Flow Architecture**
   
   Include:
   - Mermaid sequence diagram
   - Component overview
   - Data flow (A→B→C→D pattern)
   - API endpoints
   - Database schema

3. **Add API Documentation**
   
   - Request/response examples
   - Status codes
   - Error handling

4. **Add Developer Guide**
   
   - How to test locally
   - How to send test invitations
   - How to debug issues

5. **Link from Main README**
   
   Update [README.md](file:///Users/jplopez/projects/vendor-mdm-portal/README.md):
   ```markdown
   ## Features
   
   - [Invitation-Based Onboarding](./docs/features/invitation-flow.md)
   ```

#### Acceptance Criteria:
- ✅ Documentation file created
- ✅ Includes architecture diagram
- ✅ API endpoints documented
- ✅ Linked from main README

---

## Progress Tracking

### Phase 1: Critical Fixes
- [ ] Task 1.1: Add Project Reference & Remove Duplicates
- [ ] Task 1.2: Create Solution File

### Phase 2: Testing Infrastructure
- [ ] Task 2.1: Create Test Project Structure
- [ ] Task 2.2: Write Invitation Service Unit Tests
- [ ] Task 2.3: Write Integration Tests

### Phase 3: Security & Production Readiness
- [ ] Task 3.1: Implement Authentication/Authorization
- [ ] Task 3.2: Add Test Execution to CI

### Phase 4: Documentation
- [ ] Task 4.1: Create Invitation Flow Documentation

---

## Definition of Done

- ✅ All 8 tasks completed
- ✅ All tests passing (>80% coverage)
- ✅ Authentication working
- ✅ CI running tests before deployment
- ✅ Documentation up to date
- ✅ Architecture compliance: 100%
- ✅ Code review passed
- ✅ Deployed to dev environment successfully

---

**Once completed, update the [review document](file:///Users/jplopez/projects/vendor-mdm-portal/reviews/invitation-flow-review.md) with new score and completion date.**
