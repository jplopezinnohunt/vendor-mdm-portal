# Core.Framework Governance Rules

**Version**: 1.0.0  
**Status**: ENFORCED  
**Last Updated**: 2026-02-03

---

## 🎯 Purpose

`VendorMdm.Core.Framework` is the **shared foundation** for all MDM applications (VendorMDM, EmployeeMDM, ProjectMDM, etc.). It provides:

- ✅ Security (Authentication, Authorization, Roles)
- ✅ Resilience (Circuit Breaker, Retry, Bulkhead)
- ✅ Logging (Structured logging with Serilog)
- ✅ Health Checks (Database, Blob, ServiceBus)
- ✅ File Storage (Azure Blob abstraction)
- ✅ Audit Logging (Ontology-driven)
- ✅ Observability (Distributed Tracing)
- ✅ Ontology Framework (Domain concepts)

---

## 🔒 Protection Rules

### ❌ FORBIDDEN (Build will fail)

1. **Apps CANNOT implement Core interfaces directly**
   ```csharp
   // ❌ WRONG
   public class MyAuthService : IAuthenticationService { }
   ```

2. **Apps CANNOT inherit from Core classes**
   ```csharp
   // ❌ WRONG
   public class MyLogger : StructuredLogger { }
   ```

3. **Apps CANNOT modify Core constants**
   ```csharp
   // ❌ WRONG
   CoreRoles.SystemAdmin = "NewValue";
   ```

4. **Apps CANNOT add dependencies to Core**
   - Core has minimal dependencies
   - Apps depend on Core, not vice versa

### ✅ ALLOWED (Extension pattern)

1. **Apps CAN create extension methods**
   ```csharp
   // ✅ CORRECT
   public static class AuthenticationExtensions
   {
       public static async Task<Result<VendorData>> GetVendorDataAsync(
           this IAuthenticationService auth, Guid vendorId)
       {
           // App-specific logic
       }
   }
   ```

2. **Apps CAN create adapters/wrappers**
   ```csharp
   // ✅ CORRECT
   public class VendorAuthAdapter
   {
       private readonly IAuthenticationService _auth;
       
       public VendorAuthAdapter(IAuthenticationService auth)
       {
           _auth = auth;
       }
       
       public async Task<Result> AuthenticateVendorAsync(string email)
       {
           return await _auth.AuthenticateAsync(email, "MagicLink");
       }
   }
   ```

3. **Apps CAN compose Core services**
   ```csharp
   // ✅ CORRECT
   public class VendorService
   {
       private readonly IAuthenticationService _auth;
       private readonly IAuditLogService _audit;
       private readonly IStructuredLogger _logger;
       
       public VendorService(
           IAuthenticationService auth,
           IAuditLogService audit,
           IStructuredLogger logger)
       {
           _auth = auth;
           _audit = audit;
           _logger = logger;
       }
   }
   ```

4. **Apps CAN configure Core via options**
   ```csharp
   // ✅ CORRECT
   services.AddCoreFramework(configuration, "VendorMDM", options =>
   {
       options.EnableDistributedTracing = true;
       options.CacheProvider = CacheProvider.Redis;
       options.LogLevel = LogLevel.Information;
   });
   ```

---

## 📋 Change Process

### When to Modify Core

Core should be modified when:
- ✅ Multiple apps need the same functionality
- ✅ Functionality is cross-cutting (security, logging, etc.)
- ✅ Functionality is stable and well-understood
- ✅ Change benefits ALL apps

Core should NOT be modified for:
- ❌ App-specific business logic
- ❌ Experimental features
- ❌ One-off requirements
- ❌ Temporary workarounds

### How to Propose Changes

1. **Create ADR (Architecture Decision Record)**
   ```markdown
   # ADR-XXX: Add Multi-Region Support to IAuthenticationService
   
   ## Status
   Proposed
   
   ## Context
   All apps need to support multi-region deployments.
   
   ## Decision
   Add `region` parameter to IAuthenticationService.AuthenticateAsync()
   
   ## Consequences
   - Breaking change (requires version bump)
   - All apps must update
   - Benefits all apps
   ```

2. **Submit PR to Core.Framework repository**
   - Branch: `feature/multi-region-auth`
   - Requires 2 approvals (Architecture Team)
   - Requires CI/CD passing
   - Requires documentation update

3. **Version Bump (Semantic Versioning)**
   - **Patch** (1.0.X): Bug fixes, no breaking changes
   - **Minor** (1.X.0): New features, backward compatible
   - **Major** (X.0.0): Breaking changes

4. **Publish to NuGet**
   - Private Azure Artifacts feed
   - Apps opt-in to new version

5. **Migration Guide**
   - Document breaking changes
   - Provide migration examples
   - Deprecation timeline (if applicable)

---

## 🛡️ Enforcement Mechanisms

### 1. Roslyn Analyzers (Build-time)

Core.Framework includes Roslyn analyzers that enforce rules:

```
Error CORE001: Cannot implement Core interface 'IAuthenticationService' in app 'VendorMdm.Api'
Error CORE002: Cannot inherit from Core class 'StructuredLogger' in app 'VendorMdm.Api'
Error CORE003: Cannot modify Core constant 'CoreRoles.SystemAdmin'
```

### 2. Directory.Build.props (Compile-time)

```xml
<PropertyGroup>
  <!-- Core is immutable -->
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <WarningLevel>5</WarningLevel>
  
  <!-- Require XML documentation -->
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  
  <!-- API compatibility checks -->
  <ApiCompatibilityEnabled>true</ApiCompatibilityEnabled>
</PropertyGroup>
```

### 3. Branch Protection (Repository-level)

- Require 2 approvals for PRs
- Require CI/CD passing
- Require architecture team review
- No direct commits to `main`

### 4. Emergency Bypass (Use with caution)

For emergencies only, you can bypass protection:

```bash
# EMERGENCY ONLY - Logs are audited
EMERGENCY_MODE=true dotnet build
```

**When to use**:
- Production is down
- Critical security fix
- No time for formal process

**After emergency**:
- Create ADR documenting the change
- Submit PR for review
- Clean up technical debt

---

## 📊 Governance Metrics

### Tracked Metrics

1. **Core Stability**
   - Number of breaking changes per quarter
   - Target: < 1 per quarter

2. **App Adoption**
   - % of apps on latest Core version
   - Target: > 80% within 1 month

3. **Extension Pattern Usage**
   - Number of extensions vs Core modifications
   - Target: 10:1 ratio (10 extensions per 1 Core change)

4. **Emergency Bypasses**
   - Number of emergency bypasses
   - Target: < 1 per month

---

## 🎓 Best Practices

### DO ✅

1. **Use Dependency Injection**
   ```csharp
   // ✅ CORRECT
   public class VendorService
   {
       private readonly IAuthenticationService _auth;
       
       public VendorService(IAuthenticationService auth)
       {
           _auth = auth;
       }
   }
   ```

2. **Use Extension Methods for App-Specific Logic**
   ```csharp
   // ✅ CORRECT
   public static class VendorAuthExtensions
   {
       public static async Task<bool> IsVendorApprovedAsync(
           this IAuthorizationService auth, Guid vendorId)
       {
           // App-specific logic
       }
   }
   ```

3. **Use Composition Over Inheritance**
   ```csharp
   // ✅ CORRECT
   public class VendorAuditService
   {
       private readonly IAuditLogService _audit;
       
       public async Task LogVendorCreatedAsync(Vendor vendor)
       {
           await _audit.LogAsync("Vendor", vendor.Id, "Created", ...);
       }
   }
   ```

### DON'T ❌

1. **Don't Implement Core Interfaces**
   ```csharp
   // ❌ WRONG
   public class VendorAuthService : IAuthenticationService
   {
       // This will fail to build
   }
   ```

2. **Don't Inherit from Core Classes**
   ```csharp
   // ❌ WRONG
   public class VendorLogger : StructuredLogger
   {
       // This will fail to build
   }
   ```

3. **Don't Modify Core Constants**
   ```csharp
   // ❌ WRONG
   CoreRoles.SystemAdmin = "SuperAdmin";
   ```

---

## 📚 Resources

- [CONTRIBUTING.md](./CONTRIBUTING.md) - How to extend Core
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Core design principles
- [ADR Directory](./docs/adr/) - Architecture Decision Records
- [API Documentation](./docs/api/) - Core API reference

---

## 🤝 Ownership

**Architecture Team**:
- Reviews all Core changes
- Approves ADRs
- Maintains governance rules
- Publishes new versions

**App Teams**:
- Use Core services
- Create extensions
- Propose Core changes via ADR
- Migrate to new versions

---

## ✅ Compliance Checklist

Before merging to Core:

- [ ] ADR created and approved
- [ ] 2 architecture team approvals
- [ ] CI/CD passing (all tests green)
- [ ] API compatibility check passing
- [ ] Documentation updated
- [ ] Migration guide created (if breaking change)
- [ ] Version bumped (semver)
- [ ] Changelog updated

---

**Last Review**: 2026-02-03  
**Next Review**: 2026-03-03 (monthly)  
**Owner**: Architecture Team
