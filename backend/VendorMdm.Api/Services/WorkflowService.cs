using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Shared.Models;

namespace VendorMdm.Api.Services;

public interface IWorkflowService
{
    Task<List<WorkflowAction>> GetAllowedActionsAsync(string stepName, Guid workflowDefinitionId);
    Task<bool> CanPerformActionAsync(string stepName, string actionName, Guid workflowDefinitionId);
}

/// <summary>
/// Service to interact with the Canonical Workflow Model.
/// This service is DESCRIPTIVE: It reads the workflow definitions to tell the UI/App what is possible.
/// It does not enforce runtime execution logic (the Code does that) but validates against the blueprint.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly SqlDbContext _context;

    public WorkflowService(SqlDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowAction>> GetAllowedActionsAsync(string stepName, Guid workflowDefinitionId)
    {
        var step = await _context.WorkflowSteps
            .Include(s => s.AllowedActions)
            .FirstOrDefaultAsync(s => s.StepName == stepName && s.WorkflowDefinitionId == workflowDefinitionId);

        if (step == null) return new List<WorkflowAction>();

        return step.AllowedActions;
    }

    public async Task<bool> CanPerformActionAsync(string stepName, string actionName, Guid workflowDefinitionId)
    {
        var actions = await GetAllowedActionsAsync(stepName, workflowDefinitionId);
        return actions.Any(a => a.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase));
    }
}
