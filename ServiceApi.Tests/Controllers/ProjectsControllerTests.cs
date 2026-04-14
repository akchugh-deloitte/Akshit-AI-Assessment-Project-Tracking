using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceApi.API.Controllers;
using ServiceApi.API.DTOs;
using ServiceApi.API.Services;
using Xunit;

namespace ServiceApi.Tests.Controllers;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _service = new();
    private ProjectsController CreateController() => new ProjectsController(_service.Object);

    [Fact]
    public async Task GetAll_ReturnsOk_WithProjects()
    {
        // Arrange
        var items = new List<ProjectResponse>
        {
            new ProjectResponse(1, "A", "Desc", "Active", System.DateTime.UtcNow, 0),
            new ProjectResponse(2, "B", null, "Archived", System.DateTime.UtcNow, 3),
        };
        _service.Setup(s => s.GetAllAsync(default)).ReturnsAsync(items);
        var controller = CreateController();

        // Act
        var action = await controller.GetAll();

        // Assert
        var ok = action.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        // Arrange
        var item = new ProjectResponse(10, "X", "Y", "Active", System.DateTime.UtcNow, 5);
        _service.Setup(s => s.GetByIdAsync(10, default)).ReturnsAsync(item);
        var controller = CreateController();

        // Act
        var action = await controller.GetById(10);

        // Assert
        var ok = action.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _service.Setup(s => s.GetByIdAsync(123, default)).ReturnsAsync((ProjectResponse?)null);
        var controller = CreateController();

        // Act
        var action = await controller.GetById(123);

        // Assert
        action.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithCreatedEntity()
    {
        // Arrange
        var req = new CreateProjectRequest("New", "Desc");
        var created = new ProjectResponse(99, "New", "Desc", "Active", System.DateTime.UtcNow, 0);
        _service.Setup(s => s.CreateAsync(req, default)).ReturnsAsync(created);
        var controller = CreateController();

        // Act
        var action = await controller.Create(req);

        // Assert
        var createdAt = action.Result as CreatedAtActionResult;
        createdAt.Should().NotBeNull();
        createdAt!.ActionName.Should().Be(nameof(ProjectsController.GetById));
        createdAt.RouteValues!["id"].Should().Be(created.Id);
        createdAt.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUpdated()
    {
        // Arrange
        var req = new UpdateProjectRequest("NN", "DD", ServiceApi.API.Models.ProjectStatus.Inactive);
        var updated = new ProjectResponse(5, "NN", "DD", "Inactive", System.DateTime.UtcNow, 1);
        _service.Setup(s => s.UpdateAsync(5, req, default)).ReturnsAsync(updated);
        var controller = CreateController();

        // Act
        var action = await controller.Update(5, req);

        // Assert
        var ok = action.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceReturnsNull()
    {
        // Arrange
        var req = new UpdateProjectRequest(null, null, null);
        _service.Setup(s => s.UpdateAsync(404, req, default)).ReturnsAsync((ProjectResponse?)null);
        var controller = CreateController();

        // Act
        var action = await controller.Update(404, req);

        // Assert
        action.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        _service.Setup(s => s.DeleteAsync(7, default)).ReturnsAsync(DeleteProjectResult.Deleted);
        var controller = CreateController();

        // Act
        var action = await controller.Delete(7);

        // Assert
        action.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _service.Setup(s => s.DeleteAsync(77, default)).ReturnsAsync(DeleteProjectResult.NotFound);
        var controller = CreateController();

        // Act
        var action = await controller.Delete(77);

        // Assert
        action.Should().BeOfType<NotFoundResult>();
    }
}
