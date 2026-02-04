# Testing Standard

**Category**: Core Development
**Pattern #**: 9
**Status**: MANDATORY
**Priority**: 🔴 CRITICAL

---

## Definition

All code MUST have appropriate test coverage before deployment. Tests are a deployment gate - no untested code in production.

---

## Coverage Requirements

| Layer | Minimum Coverage | Test Type |
|-------|------------------|-----------|
| **Domain Concepts** | 90% | Unit Tests |
| **Services** | 80% | Unit + Integration |
| **Controllers** | 70% | Integration |
| **Repositories** | 80% | Integration |
| **Overall** | 75% | Combined |

---

## Test Categories

### 1. Unit Tests (MANDATORY)

**Purpose**: Test business logic in isolation
**Location**: `backend/VendorMdm.Tests/Unit/`

```csharp
// ✅ CORRECT: Test Concept logic
[Fact]
public void VendorConcept_WhenStatusIsDraft_CanTransitionToSubmitted()
{
    // Arrange
    var vendor = new VendorConcept(CreateTestVendor(status: VendorStatus.Draft));

    // Act
    var result = vendor.TransitionTo(VendorStatus.Submitted);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(VendorStatus.Submitted, vendor.Status);
}

// ✅ CORRECT: Test failure cases
[Fact]
public void VendorConcept_WhenStatusIsApproved_CannotTransitionToDraft()
{
    // Arrange
    var vendor = new VendorConcept(CreateTestVendor(status: VendorStatus.Approved));

    // Act
    var result = vendor.TransitionTo(VendorStatus.Draft);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Invalid transition", result.Error);
}
```

### 2. Integration Tests (MANDATORY)

**Purpose**: Test full request/response cycle
**Location**: `backend/VendorMdm.Tests/Integration/`

```csharp
[Fact]
public async Task CreateVendor_WithValidData_ReturnsCreatedResponse()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new CreateVendorRequest { LegalName = "Test Corp" };

    // Act
    var response = await client.PostAsJsonAsync("/api/vendors", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var vendor = await response.Content.ReadFromJsonAsync<VendorDto>();
    Assert.NotNull(vendor);
    Assert.Equal("Test Corp", vendor.LegalName);
}
```

### 3. Contract Tests (RECOMMENDED)

**Purpose**: Verify API contracts don't break
**Location**: `backend/VendorMdm.Tests/Contracts/`

```csharp
[Fact]
public void VendorDto_MatchesExpectedSchema()
{
    var dto = new VendorDto { Id = Guid.NewGuid(), LegalName = "Test" };
    var json = JsonSerializer.Serialize(dto);

    // Verify required fields exist
    Assert.Contains("id", json);
    Assert.Contains("legalName", json);
}
```

---

## Test Naming Convention

```
[MethodUnderTest]_[Scenario]_[ExpectedResult]
```

**Examples**:
- `GetVendor_WhenVendorExists_ReturnsVendor`
- `GetVendor_WhenVendorDeleted_ReturnsNotFound`
- `CreateVendor_WithInvalidEmail_ReturnsValidationError`

---

## Test Data Management

### 1. Test Fixtures

```csharp
public static class TestDataFactory
{
    public static Vendor CreateTestVendor(
        string status = VendorStatus.Draft,
        string legalName = "Test Vendor LLC")
    {
        return new Vendor
        {
            Id = Guid.NewGuid(),
            LegalName = legalName,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### 2. Database Isolation

```csharp
// ✅ CORRECT: Use in-memory database for unit tests
services.AddDbContext<SqlDbContext>(options =>
    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

// ✅ CORRECT: Use transactions for integration tests
public class IntegrationTestBase : IAsyncLifetime
{
    private IDbContextTransaction _transaction;

    public async Task InitializeAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
    }
}
```

---

## Mocking Guidelines

### What to Mock

| Mock | Don't Mock |
|------|------------|
| External APIs (SAP, Salesforce) | Domain Concepts |
| Email services | Business logic |
| File storage | In-memory databases |
| Time (`IDateTimeProvider`) | Simple value objects |

### Mock Pattern

```csharp
// ✅ CORRECT: Mock external dependency
var mockSapClient = new Mock<ISapClient>();
mockSapClient
    .Setup(x => x.CreateVendorAsync(It.IsAny<SapVendorRequest>()))
    .ReturnsAsync(Result<string>.Success("SAP001"));

// ❌ FORBIDDEN: Mock domain logic
var mockVendorConcept = new Mock<VendorConcept>(); // NO!
```

---

## CI/CD Integration

### Pre-Commit (Local)

```bash
# Run unit tests before commit
dotnet test backend/VendorMdm.Tests --filter "Category=Unit"
```

### Pull Request (CI)

```yaml
# .github/workflows/test.yml
- name: Run Tests
  run: |
    dotnet test backend/VendorMdm.Tests \
      --configuration Release \
      --collect:"XPlat Code Coverage" \
      --results-directory ./coverage

- name: Check Coverage
  run: |
    coverage=$(cat coverage/*/coverage.cobertura.xml | grep -oP 'line-rate="\K[^"]+')
    if (( $(echo "$coverage < 0.75" | bc -l) )); then
      echo "Coverage $coverage is below 75% threshold"
      exit 1
    fi
```

---

## Test Pyramid

```
        /\
       /  \  E2E Tests (5%)
      /----\  - Critical user journeys only
     /      \
    /--------\  Integration Tests (25%)
   /          \  - API endpoints
  /            \  - Database operations
 /--------------\  Unit Tests (70%)
/                \  - Business logic
------------------  - Domain concepts
```

---

## Anti-Patterns

❌ Testing implementation details instead of behavior
❌ Flaky tests that depend on timing
❌ Tests that require specific environment setup
❌ Skipping tests to meet deadlines
❌ Testing private methods directly
❌ Shared mutable state between tests

---

## Agent Behavior

**Before PR**:
1. ✅ Verify test coverage meets thresholds
2. ✅ Run all tests locally
3. ✅ Add tests for new functionality
4. ✅ Update tests for modified functionality

**When Tests Fail**:
1. ✅ Fix the failing test (don't skip)
2. ✅ Document why test was failing
3. ✅ Verify fix doesn't break other tests

---

## Reference

- **Test Project**: `backend/VendorMdm.Tests/`
- **Coverage Tool**: Coverlet
- **Mocking**: Moq
- **Assertions**: xUnit + FluentAssertions
- **Golden Rules**: Section 4, Category 2
