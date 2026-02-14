using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VendorMdm.Api.Data;
using VendorMdm.Api.Services;
using VendorMdm.Api.Tests.Helpers;
using VendorMdm.Shared.Models;
using Xunit;

namespace VendorMdm.Api.Tests.Services;

public class WorkflowServiceTests : TestBase
{
    private (WorkflowService service, SqlDbContext context) CreateService()
    {
        var context = CreateInMemoryDbContext();
        var service = new WorkflowService(context);
        return (service, context);
    }

    private async Task<(WorkflowDefinition definition, WorkflowStep step)> CreateTestWorkflow(SqlDbContext context)
    {
        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "VendorOnboarding",
            Version = "1.0",
            IsActive = true
        };
        context.WorkflowDefinitions.Add(definition);

        var step = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = definition.Id,
            StepName = "PendingReview",
            OrderIndex = 1,
            AllowedActions = new List<WorkflowAction>
            {
                new WorkflowAction
                {
                    Id = Guid.NewGuid(),
                    ActionName = "Approve",
                    TargetStepName = "Approved"
                },
                new WorkflowAction
                {
                    Id = Guid.NewGuid(),
                    ActionName = "Reject",
                    TargetStepName = "Rejected"
                },
                new WorkflowAction
                {
                    Id = Guid.NewGuid(),
                    ActionName = "RequestInfo",
                    TargetStepName = "PendingInfo"
                }
            }
        };
        context.WorkflowSteps.Add(step);

        await context.SaveChangesAsync();
        return (definition, step);
    }

    #region GetAllowedActionsAsync Tests

    [Fact]
    public async Task GetAllowedActionsAsync_ExistingStep_ReturnsActions()
    {
        // Arrange
        var (service, context) = CreateService();
        var (definition, step) = await CreateTestWorkflow(context);

        // Act
        var result = await service.GetAllowedActionsAsync("PendingReview", definition.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.ActionName == "Approve");
        result.Should().Contain(a => a.ActionName == "Reject");
        result.Should().Contain(a => a.ActionName == "RequestInfo");
    }

    [Fact]
    public async Task GetAllowedActionsAsync_NonExistingStep_ReturnsEmptyList()
    {
        // Arrange
        var (service, context) = CreateService();
        var (definition, _) = await CreateTestWorkflow(context);

        // Act
        var result = await service.GetAllowedActionsAsync("NonExistingStep", definition.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllowedActionsAsync_NonExistingWorkflow_ReturnsEmptyList()
    {
        // Arrange
        var (service, context) = CreateService();
        await CreateTestWorkflow(context);

        // Act
        var result = await service.GetAllowedActionsAsync("PendingReview", Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CanPerformActionAsync Tests

    [Fact]
    public async Task CanPerformActionAsync_ValidAction_ReturnsTrue()
    {
        // Arrange
        var (service, context) = CreateService();
        var (definition, _) = await CreateTestWorkflow(context);

        // Act
        var result = await service.CanPerformActionAsync("PendingReview", "Approve", definition.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanPerformActionAsync_ValidActionCaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var (service, context) = CreateService();
        var (definition, _) = await CreateTestWorkflow(context);

        // Act
        var result = await service.CanPerformActionAsync("PendingReview", "approve", definition.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanPerformActionAsync_InvalidAction_ReturnsFalse()
    {
        // Arrange
        var (service, context) = CreateService();
        var (definition, _) = await CreateTestWorkflow(context);

        // Act
        var result = await service.CanPerformActionAsync("PendingReview", "Delete", definition.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanPerformActionAsync_NonExistingStep_ReturnsFalse()
    {
        // Arrange
        var (service, context) = CreateService();
        var (definition, _) = await CreateTestWorkflow(context);

        // Act
        var result = await service.CanPerformActionAsync("NonExistingStep", "Approve", definition.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanPerformActionAsync_NonExistingWorkflow_ReturnsFalse()
    {
        // Arrange
        var (service, context) = CreateService();
        await CreateTestWorkflow(context);

        // Act
        var result = await service.CanPerformActionAsync("PendingReview", "Approve", Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
