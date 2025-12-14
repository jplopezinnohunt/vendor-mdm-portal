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

    // Canonical Entities
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorInvitationCanonical> VendorInvitationsCanonical { get; set; }
    public DbSet<ChangeRequestCanonical> ChangeRequestsCanonical { get; set; }
    public DbSet<SapIdMapping> SapIdMappings { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure JSON columns for Hybrid Relational-Document Model
        ConfigureJsonColumns(modelBuilder);

        // Configure Canonical Entities
        ConfigureCanonicalEntities(modelBuilder);

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
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("{}");

        // VendorApplication JSON configuration
        modelBuilder.Entity<VendorApplication>()
            .Property(e => e.Attributes)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("{}");

        // ChangeRequest JSON configuration
        modelBuilder.Entity<ChangeRequest>()
            .Property(e => e.Attributes)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("{}");

        // Attachment JSON configuration
        modelBuilder.Entity<Attachment>()
            .Property(e => e.Attributes)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("{}");

        // UserRole JSON configuration
        modelBuilder.Entity<UserRole>()
            .Property(e => e.Attributes)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValue("{}");

        // WorkflowState JSON configuration
        modelBuilder.Entity<WorkflowState>()
            .Property(e => e.Attributes)
            .HasColumnType("nvarchar(max)")
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
                .HasColumnType("nvarchar(max)")
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
                .HasColumnType("nvarchar(max)")
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
                .HasColumnType("nvarchar(max)")
                .IsRequired()
                .HasDefaultValue("{}");
        });

        // SapIdMapping - Anti-corruption layer
        modelBuilder.Entity<SapIdMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one SAP ID per canonical entity per environment
            entity.HasIndex(e => new { e.EntityType, e.SapId, e.SapEnvironment })
                .IsUnique();
            
            // Index for canonical ID lookups
            entity.HasIndex(e => new { e.CanonicalEntityId, e.EntityType });
        });
    }
}

