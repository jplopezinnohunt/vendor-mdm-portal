using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorMdm.Shared.Models;

/// <summary>
/// Canonical Workflow Definition.
/// Acts as a "Blueprint" or "Descriptive Model" of a business process (e.g., "VendorOnboarding v1").
/// This model communicates the intended flow, steps, and actors clearly.
/// </summary>
public class WorkflowDefinition : CanonicalEntityBase
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Vendor Onboarding"

    [MaxLength(50)]
    public string Domain { get; set; } = "Vendor"; // e.g., "Vendor", "HR", "Finance"

    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0"; // Semantic Versioning

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation Property
    public List<WorkflowStep> Steps { get; set; } = new();
}

/// <summary>
/// Represents a distinct state or miletsone in the workflow.
/// e.g., "Draft", "Manager Approval", "SAP Integration".
/// </summary>
public class WorkflowStep : CanonicalEntityBase
{
    public Guid WorkflowDefinitionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string StepName { get; set; } = string.Empty; // e.g., "LegalReview"

    [Required]
    public int OrderIndex { get; set; } // 1, 2, 3...

    [MaxLength(50)]
    public string StepType { get; set; } = "Task"; // Task, Approval, Automated, Decision

    // Descriptive Metadata (not driving logic, but explaining it)
    public bool IsFinal { get; set; } = false;
    
    // Navigation
    public List<WorkflowAction> AllowedActions { get; set; } = new();
    
    /// <summary>
    /// Roles responsible for this step (Descriptive).
    /// </summary>
    public List<WorkflowRoleBinding> RoleBindings { get; set; } = new();
}

/// <summary>
/// Represents an action that can be taken at a specific step.
/// e.g., "Approve", "Reject", "Request Info".
/// Maps to application UI buttons.
/// </summary>
public class WorkflowAction : CanonicalEntityBase
{
    public Guid WorkflowStepId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionName { get; set; } = string.Empty; // e.g., "Approve"

    [MaxLength(100)]
    public string TargetStepName { get; set; } = string.Empty; // Where does this go? e.g., "SAP Integration"

    [MaxLength(50)]
    public string ButtonLabel { get; set; } = string.Empty; // UI Hint

    [MaxLength(20)]
    public string ButtonStyle { get; set; } = "Primary"; // Primary, Danger, Default
}


/// <summary>
/// Binds an abstract Workflow Role (e.g., "Approver") to a specific step.
/// Descriptive model of "Who does what".
/// </summary>
public class WorkflowRoleBinding : CanonicalEntityBase
{
    public Guid WorkflowStepId { get; set; }

    [Required]
    [MaxLength(50)]
    public string WorkflowRole { get; set; } = string.Empty; // e.g., "LegalApprover"

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Metadata for fields involved in a step.
/// Validates data entry and controls UI rendering prompts.
/// </summary>
public class WorkflowFieldDefinition : CanonicalEntityBase
{
    public Guid WorkflowStepId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty; // e.g., "TaxId"

    [MaxLength(50)]
    public string FieldType { get; set; } = "Text"; // Text, Number, Date, File, Boolean

    public bool IsRequired { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;
    public bool IsHidden { get; set; } = false;

    [MaxLength(500)]
    public string ValidationRegex { get; set; } = string.Empty;

    [MaxLength(50)]
    public string GroupName { get; set; } = string.Empty; // For UI layout grouping
}
