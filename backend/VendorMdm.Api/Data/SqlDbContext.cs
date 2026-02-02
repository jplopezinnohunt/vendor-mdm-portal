using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Models;
using VendorMdm.Shared.Models;
using VendorMdm.Shared.Mapping;

namespace VendorMdm.Api.Data;

public class SqlDbContext : DbContext
{
    public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options) { }

    public DbSet<ChangeRequest> ChangeRequests { get; set; }
    public DbSet<VendorApplication> VendorApplications { get; set; }
    public DbSet<VendorInvitation> VendorInvitations { get; set; }
    public DbSet<WorkflowState> WorkflowStates { get; set; }
    public DbSet<SapEnvironment> SapEnvironments { get; set; }
    public DbSet<UserRole> UsersAndRoles { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventParticipant> EventParticipants { get; set; }

    // Canonical Entities
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorInvitationCanonical> VendorInvitationsCanonical { get; set; }
    public DbSet<ChangeRequestCanonical> ChangeRequestsCanonical { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Fund> Funds { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<User> Users { get; set; } /* Canonical User entity */
    public DbSet<ExternalSystemMapping> ExternalSystemMappings { get; set; }
    
    // Pattern 16: Audit Trail & Temporal
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Workflow Canonical Model
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowAction> WorkflowActions { get; set; }
    public DbSet<WorkflowRoleBinding> WorkflowRoleBindings { get; set; }
    public DbSet<WorkflowFieldDefinition> WorkflowFieldDefinitions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure JSON columns for Hybrid Relational-Document Model
        ConfigureJsonColumns(modelBuilder);

        // Configure Canonical Entities
        ConfigureCanonicalEntities(modelBuilder);
        
        // Configure Workflow Canonical Entities
        ConfigureWorkflowEntities(modelBuilder);
        
        // Configure Audit Trail (Pattern 16)
        ConfigureAuditLog(modelBuilder);

        // Seed Workflow States
        modelBuilder.Entity<WorkflowState>().HasData(
            new WorkflowState { StateName = "Draft", Description = "Initial draft" },
            new WorkflowState { StateName = "Submitted", Description = "Submitted for approval" },
            new WorkflowState { StateName = "Approved", Description = "Approved by admin" },
            new WorkflowState { StateName = "Integrated", Description = "Synced to SAP" }
        );

        // Seed Environments
        modelBuilder.Entity<SapEnvironment>().HasData(
            new SapEnvironment { EnvironmentCode = "D01", Description = "Development" },
            new SapEnvironment { EnvironmentCode = "Q01", Description = "Quality Assurance" },
            new SapEnvironment { EnvironmentCode = "P01", Description = "Production" }
        );
    }

    /// <summary>
    /// Configures JSON columns per Hybrid Relational-Document Model architectural standard.
    /// All semi-structured data is stored in nvarchar(max) Attributes columns.
    /// </summary>
    private void ConfigureJsonColumns(ModelBuilder modelBuilder)
    {
        // VendorInvitation JSON configuration
        modelBuilder.Entity<VendorInvitation>()
            .Property(e => e.Attributes)
            
            .IsRequired()
            .HasDefaultValue("{}");

        // VendorApplication JSON configuration
        modelBuilder.Entity<VendorApplication>()
            .Property(e => e.Attributes)
            
            .IsRequired()
            .HasDefaultValue("{}");

        // ChangeRequest JSON configuration
        modelBuilder.Entity<ChangeRequest>()
            .Property(e => e.Attributes)
            
            .IsRequired()
            .HasDefaultValue("{}");

        // Attachment JSON configuration
        modelBuilder.Entity<Attachment>()
            .Property(e => e.Attributes)
            
            .IsRequired()
            .HasDefaultValue("{}");

        // UserRole JSON configuration
        modelBuilder.Entity<UserRole>()
            .Property(e => e.Attributes)
            
            .IsRequired()
            .HasDefaultValue("{}");

        // WorkflowState JSON configuration
        modelBuilder.Entity<WorkflowState>()
            .Property(e => e.Attributes)
            
            .IsRequired()
            .HasDefaultValue("{}");

        // Event JSON configuration
        modelBuilder.Entity<Event>()
            .Property(e => e.Attributes)
            .IsRequired()
            .HasDefaultValue("{}");

        // EventParticipant JSON configuration
        modelBuilder.Entity<EventParticipant>()
            .Property(e => e.Attributes)
            .IsRequired()
            .HasDefaultValue("{}");
    }

    /// <summary>
    /// Configure canonical entities per Canonical Domain Model standard.
    /// All canonical entities have: Id, EntityVersion, Status, SourceSystem, Data, timestamps.
    /// </summary>
    private void ConfigureCanonicalEntities(ModelBuilder modelBuilder)
    {
        // Vendor canonical entity
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LegalName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PrimaryContactEmail);
            entity.HasIndex(e => e.SourceSystem);
            
            entity.Property(e => e.Data)
                
                .IsRequired()
                .HasDefaultValue("{}");
        });

        // VendorInvitationCanonical
        modelBuilder.Entity<VendorInvitationCanonical>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvitationToken).IsUnique();
            entity.HasIndex(e => e.PrimaryContactEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAt);
            
            entity.Property(e => e.Data)
                
                .IsRequired()
                .HasDefaultValue("{}");
        });

        // ChangeRequestCanonical
        modelBuilder.Entity<ChangeRequestCanonical>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VendorId); // Canonical Vendor ID (not SAP!)
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequesterId);
            
            entity.Property(e => e.Data)
                
                .IsRequired()
                .HasDefaultValue("{}");
        });

        // ExternalSystemMapping - Anti-corruption layer for multi-system integration
        modelBuilder.Entity<ExternalSystemMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one external system ID per canonical entity per system+environment
            entity.HasIndex(e => new { e.EntityType, e.ExternalSystemId, e.SystemName, e.SystemEnvironment })
                .IsUnique()
                .HasDatabaseName("IX_ExternalSystemMapping_Unique");
            
            // Index for canonical ID lookups
            entity.HasIndex(e => new { e.CanonicalEntityId, e.EntityType, e.SystemName })
                .HasDatabaseName("IX_ExternalSystemMapping_Canonical");
        });

        // Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.EmployeeId);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectCode);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        // Fund
        modelBuilder.Entity<Fund>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FundCode);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.TaxId);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.AzureAdObjectId); // Fast lookup for SSO
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });
    }
    /// <summary>
    /// Configure Workflow Canonical Entities (Descriptive Model).
    /// </summary>
    private void ConfigureWorkflowEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Steps).WithOne().HasForeignKey(s => s.WorkflowDefinitionId);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.AllowedActions).WithOne().HasForeignKey(a => a.WorkflowStepId);
            entity.HasMany(e => e.RoleBindings).WithOne().HasForeignKey(r => r.WorkflowStepId);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        modelBuilder.Entity<WorkflowAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        modelBuilder.Entity<WorkflowRoleBinding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });

        modelBuilder.Entity<WorkflowFieldDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkflowStepId);
            entity.Property(e => e.Data).IsRequired().HasDefaultValue("{}");
        });
    }
    
    /// <summary>
    /// Configure Audit Log entity (Pattern 16: Audit Trail & Temporal).
    /// Indexes optimized for compliance queries and temporal reconstruction.
    /// </summary>
    private void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Index for entity-specific queries ("Show all changes to Vendor X")
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.ChangedAt })
                .HasDatabaseName("IX_AuditLog_Entity");
            
            // Index for user activity queries ("Show all changes by User Y")
            entity.HasIndex(e => new { e.ChangedByUserId, e.ChangedAt })
                .HasDatabaseName("IX_AuditLog_User");
            
            // Index for temporal queries ("Show all changes on Date Z")
            entity.HasIndex(e => e.ChangedAt)
                .HasDatabaseName("IX_AuditLog_Timestamp");
            
            // Index for action-based queries ("Show all deletions")
            entity.HasIndex(e => new { e.Action, e.ChangedAt })
                .HasDatabaseName("IX_AuditLog_Action");
            
            // Index for tenant-based queries (Pattern 15: Multi-Tenancy)
            entity.HasIndex(e => new { e.TenantId, e.ChangedAt })
                .HasDatabaseName("IX_AuditLog_Tenant");
        });
    }
}

