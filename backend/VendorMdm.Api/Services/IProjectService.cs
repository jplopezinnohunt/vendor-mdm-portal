using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<List<Project>> GetAllProjectsAsync();
}
