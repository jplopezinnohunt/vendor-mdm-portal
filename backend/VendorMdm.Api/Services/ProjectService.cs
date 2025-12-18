using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace VendorMdm.Api.Services;

public class ProjectService : IProjectService
{
    private readonly SqlDbContext _context;
    private readonly CosmosRepository _cosmosRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        SqlDbContext context,
        CosmosRepository cosmosRepository,
        ILogger<ProjectService> logger)
    {
        _context = context;
        _cosmosRepository = cosmosRepository;
        _logger = logger;
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));

        // 1. SQL Persistence
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        project.EntityVersion = 1;
        project.SchemaVersion = "v1.0.0";
        if (string.IsNullOrEmpty(project.Status)) project.Status = "Active";
        if (string.IsNullOrEmpty(project.SourceSystem)) project.SourceSystem = SourceSystems.GetDefaultSource(typeof(Project));

        project.ValidateCanonicalFields();

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // 2. Artifact
        await _cosmosRepository.SaveArtifactAsync(project.Id.ToString(), project);

        // 3. Event
        var domainEvent = new DomainEvent
        {
            EventType = "ProjectCreated",
            EntityId = project.Id.ToString(),
            Data = project,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = project.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        project.IncrementVersion();

        _context.Projects.Update(project);
        await _context.SaveChangesAsync();

        await _cosmosRepository.SaveArtifactAsync(project.Id.ToString(), project);

        var domainEvent = new DomainEvent
        {
            EventType = "ProjectUpdated",
            EntityId = project.Id.ToString(),
            Data = project,
            Timestamp = DateTime.UtcNow,
            Source = "VendorMdm.Api",
            SchemaVersion = project.SchemaVersion
        };
        await _cosmosRepository.LogDomainEventAsync(domainEvent);

        return project;
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        return await _context.Projects.ToListAsync();
    }
}
