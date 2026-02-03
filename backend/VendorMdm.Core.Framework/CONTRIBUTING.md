# Contributing to Core.Framework

**Welcome!** This guide shows you how to extend `VendorMdm.Core.Framework` safely without modifying it.

---

## 🎯 Philosophy: Extension Over Modification

**Core Principle**: Apps should **USE** Core, not **MODIFY** Core.

```
✅ GOOD: Extend Core with app-specific logic
❌ BAD: Modify Core for app-specific needs
```

---

## 📋 Common Scenarios

### Scenario 1: I need app-specific authentication logic

**❌ WRONG: Modify Core**
```csharp
// DON'T DO THIS
// VendorMdm.Core.Framework/Security/IAuthenticationService.cs
public interface IAuthenticationService
{
    Task<Result<AuthToken>> AuthenticateAsync(string identifier, string method);
    Task<Result<VendorData>> GetVendorDataAsync(Guid vendorId); // ❌ App-specific!
}
```

**✅ CORRECT: Create Extension**
```csharp
// VendorMdm.Api/Extensions/AuthenticationExtensions.cs
public static class AuthenticationExtensions
{
    public static async Task<Result<VendorData>> GetVendorDataAsync(
        this IAuthenticationService auth,
        Guid vendorId,
        SqlDbContext context)
    {
        // App-specific logic here
        var vendor = await context.Vendors.FindAsync(vendorId);
        if (vendor == null)
            return Result.Fail<VendorData>("Vendor not found");
        
        return Result.Ok(new VendorData
        {
            Id = vendor.Id,
            LegalName = vendor.LegalName,
            Status = vendor.Status
        });
    }
}

// Usage
var vendorData = await _authService.GetVendorDataAsync(vendorId, _context);
```

---

### Scenario 2: I need custom logging for my app

**❌ WRONG: Inherit from Core**
```csharp
// DON'T DO THIS
public class VendorLogger : StructuredLogger // ❌ Inheritance forbidden
{
    public void LogVendorCreated(Vendor vendor)
    {
        // Custom logic
    }
}
```

**✅ CORRECT: Create Wrapper**
```csharp
// VendorMdm.Api/Services/VendorLoggingService.cs
public class VendorLoggingService
{
    private readonly IStructuredLogger _logger;
    
    public VendorLoggingService(IStructuredLogger logger)
    {
        _logger = logger;
    }
    
    public void LogVendorCreated(Vendor vendor)
    {
        _logger.LogInformation(
            "Vendor created",
            ("VendorId", vendor.Id),
            ("LegalName", vendor.LegalName),
            ("VendorType", vendor.VendorType),
            ("CreatedBy", vendor.CreatedBy)
        );
    }
    
    public void LogVendorApproved(Vendor vendor, string approvedBy)
    {
        _logger.LogInformation(
            "Vendor approved",
            ("VendorId", vendor.Id),
            ("ApprovedBy", approvedBy),
            ("PreviousStatus", "Pending"),
            ("NewStatus", "Approved")
        );
    }
}

// Usage
_vendorLogging.LogVendorCreated(vendor);
```

---

### Scenario 3: I need custom resilience policy

**❌ WRONG: Modify Core Policy Registry**
```csharp
// DON'T DO THIS
// VendorMdm.Core.Framework/Resilience/CorePolicyRegistry.cs
public static class CorePolicyRegistry
{
    public static IAsyncPolicy VendorSpecificPolicy => // ❌ App-specific!
        Policy.Handle<Exception>().RetryAsync(5);
}
```

**✅ CORRECT: Create App-Specific Policy**
```csharp
// VendorMdm.Api/Resilience/VendorPolicyRegistry.cs
public static class VendorPolicyRegistry
{
    // Use Core policies as base
    public static IAsyncPolicy SapVendorSyncPolicy =>
        Policy.WrapAsync(
            CorePolicyRegistry.HttpCircuitBreakerPolicy,
            CorePolicyRegistry.HttpRetryPolicy,
            Policy.TimeoutAsync(TimeSpan.FromSeconds(30)) // App-specific timeout
        );
    
    public static IAsyncPolicy EmailNotificationPolicy =>
        Policy
            .Handle<SmtpException>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Usage
await VendorPolicyRegistry.SapVendorSyncPolicy.ExecuteAsync(async () =>
{
    await _sapService.SyncVendorAsync(vendor);
});
```

---

### Scenario 4: I need to add a new Core service

**When to add to Core**:
- ✅ Multiple apps need it
- ✅ It's cross-cutting (security, logging, etc.)
- ✅ It's stable and well-understood

**Process**:

1. **Create ADR**
   ```markdown
   # ADR-005: Add INotificationService to Core
   
   ## Status
   Proposed
   
   ## Context
   All apps (VendorMDM, EmployeeMDM, ProjectMDM) need to send notifications.
   Currently each app implements its own notification logic.
   
   ## Decision
   Add INotificationService to Core.Framework with support for:
   - Email notifications
   - SMS notifications (Twilio)
   - Push notifications (Firebase)
   
   ## Consequences
   - Apps can reuse notification logic
   - Consistent notification behavior across apps
   - Breaking change: None (new service)
   - Version: 1.1.0 (minor bump)
   ```

2. **Implement Interface**
   ```csharp
   // VendorMdm.Core.Framework/Notifications/INotificationService.cs
   public interface INotificationService
   {
       Task<Result> SendEmailAsync(string to, string subject, string body);
       Task<Result> SendSmsAsync(string phoneNumber, string message);
       Task<Result> SendPushAsync(string deviceToken, string title, string body);
   }
   ```

3. **Implement Service**
   ```csharp
   // VendorMdm.Core.Framework/Notifications/NotificationService.cs
   public class NotificationService : INotificationService
   {
       private readonly IEmailService _email;
       private readonly ISmsService _sms;
       private readonly IPushService _push;
       private readonly IStructuredLogger _logger;
       
       public async Task<Result> SendEmailAsync(string to, string subject, string body)
       {
           try
           {
               await _email.SendAsync(to, subject, body);
               _logger.LogInformation("Email sent", ("To", to), ("Subject", subject));
               return Result.Ok();
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Email send failed", ("To", to));
               return Result.Fail($"Email send failed: {ex.Message}");
           }
       }
   }
   ```

4. **Add to Extensions**
   ```csharp
   // VendorMdm.Core.Framework/Extensions/ServiceCollectionExtensions.cs
   public static IServiceCollection AddCoreFramework(
       this IServiceCollection services,
       IConfiguration configuration,
       string appName)
   {
       // ... existing services ...
       
       // Notifications (NEW)
       services.AddSingleton<INotificationService, NotificationService>();
       
       return services;
   }
   ```

5. **Write Tests**
   ```csharp
   // VendorMdm.Core.Framework.Tests/Notifications/NotificationServiceTests.cs
   public class NotificationServiceTests
   {
       [Fact]
       public async Task SendEmailAsync_Success_ReturnsOk()
       {
           // Arrange
           var emailService = new Mock<IEmailService>();
           var notificationService = new NotificationService(emailService.Object, ...);
           
           // Act
           var result = await notificationService.SendEmailAsync("test@example.com", "Subject", "Body");
           
           // Assert
           Assert.True(result.IsSuccess);
       }
   }
   ```

6. **Update Documentation**
   - API documentation
   - Usage examples
   - Migration guide (if needed)

7. **Submit PR**
   - Requires 2 approvals
   - CI/CD must pass
   - Version bump to 1.1.0

---

## 🎓 Extension Patterns

### Pattern 1: Extension Methods

**When to use**: Adding app-specific behavior to Core interfaces.

```csharp
public static class CoreExtensions
{
    public static async Task<Result<T>> WithRetryAsync<T>(
        this Task<Result<T>> task,
        int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            var result = await task;
            if (result.IsSuccess)
                return result;
            
            if (i < maxRetries - 1)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
        }
        
        return await task;
    }
}

// Usage
var result = await _authService
    .AuthenticateAsync(email, "MagicLink")
    .WithRetryAsync(maxRetries: 3);
```

### Pattern 2: Adapter/Wrapper

**When to use**: Simplifying Core interfaces for app-specific use cases.

```csharp
public class VendorAuthAdapter
{
    private readonly IAuthenticationService _auth;
    private readonly IAuthorizationService _authz;
    private readonly IStructuredLogger _logger;
    
    public VendorAuthAdapter(
        IAuthenticationService auth,
        IAuthorizationService authz,
        IStructuredLogger logger)
    {
        _auth = auth;
        _authz = authz;
        _logger = logger;
    }
    
    public async Task<Result<VendorAuthContext>> AuthenticateVendorAsync(string email)
    {
        // 1. Authenticate
        var authResult = await _auth.AuthenticateAsync(email, "MagicLink");
        if (authResult.IsFailure)
            return Result.Fail<VendorAuthContext>(authResult.Error);
        
        // 2. Check vendor role
        var hasRole = await _authz.HasRoleAsync(authResult.Value.UserId, "Vendor", "VendorMDM");
        if (!hasRole)
            return Result.Fail<VendorAuthContext>("User is not a vendor");
        
        // 3. Log
        _logger.LogInformation("Vendor authenticated", ("Email", email));
        
        // 4. Return context
        return Result.Ok(new VendorAuthContext
        {
            Token = authResult.Value,
            Email = email,
            Roles = new[] { "Vendor" }
        });
    }
}
```

### Pattern 3: Composition

**When to use**: Combining multiple Core services for complex operations.

```csharp
public class VendorOnboardingOrchestrator
{
    private readonly IAuthenticationService _auth;
    private readonly IAuditLogService _audit;
    private readonly INotificationService _notification;
    private readonly IFileStorageService _storage;
    private readonly IStructuredLogger _logger;
    
    public async Task<Result> OnboardVendorAsync(VendorOnboardingRequest request)
    {
        using var scope = _logger.BeginScope(("VendorEmail", request.Email));
        
        // 1. Create auth account
        var authResult = await _auth.CreateAccountAsync(request.Email, "Vendor");
        if (authResult.IsFailure)
            return authResult;
        
        // 2. Upload documents
        foreach (var doc in request.Documents)
        {
            var uploadResult = await _storage.UploadAsync(doc.Stream, doc.FileName, "vendor-docs");
            if (uploadResult.IsFailure)
                return uploadResult;
        }
        
        // 3. Send welcome email
        await _notification.SendEmailAsync(
            request.Email,
            "Welcome to Vendor Portal",
            "Your account has been created...");
        
        // 4. Audit log
        await _audit.LogAsync(
            "Vendor",
            authResult.Value.UserId,
            "Onboarded",
            newValues: new { Email = request.Email, Status = "Active" });
        
        _logger.LogInformation("Vendor onboarded successfully");
        
        return Result.Ok();
    }
}
```

---

## 🛠️ Development Workflow

### 1. Local Development

```bash
# Clone repository
git clone https://github.com/yourorg/vendor-mdm-portal.git
cd vendor-mdm-portal

# Create feature branch
git checkout -b feature/vendor-auth-extension

# Make changes (extensions only, not Core)
# ...

# Build
dotnet build

# Test
dotnet test

# Commit
git add .
git commit -m "feat: Add vendor authentication extension"

# Push
git push origin feature/vendor-auth-extension
```

### 2. Testing Extensions

```csharp
// VendorMdm.Api.Tests/Extensions/AuthenticationExtensionsTests.cs
public class AuthenticationExtensionsTests
{
    [Fact]
    public async Task GetVendorDataAsync_VendorExists_ReturnsData()
    {
        // Arrange
        var authService = new Mock<IAuthenticationService>();
        var context = CreateInMemoryContext();
        var vendor = new Vendor { Id = Guid.NewGuid(), LegalName = "Acme Corp" };
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();
        
        // Act
        var result = await authService.Object.GetVendorDataAsync(vendor.Id, context);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Corp", result.Value.LegalName);
    }
}
```

---

## 📚 Resources

- [GOVERNANCE.md](./GOVERNANCE.md) - Core protection rules
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Core design principles
- [API Documentation](./docs/api/) - Core API reference
- [Examples](./examples/) - Extension examples

---

## 🤝 Getting Help

**Questions?**
- Create GitHub Discussion
- Tag `@architecture-team`

**Found a bug in Core?**
- Create GitHub Issue
- Tag `bug` and `core`

**Want to propose Core change?**
- Create ADR
- Submit PR with ADR
- Request architecture team review

---

**Happy extending!** 🚀
