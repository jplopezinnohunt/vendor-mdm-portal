using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Models;

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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
}
