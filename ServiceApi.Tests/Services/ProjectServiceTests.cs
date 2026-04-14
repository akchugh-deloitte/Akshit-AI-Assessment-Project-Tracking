using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;
using ServiceApi.API.Services;
using ServiceApi.API.Repositories;
using ServiceApi.API.DTOs;
using ServiceApi.API.Models;

namespace ServiceApi.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _repo = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _sut = new ProjectService(_repo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedResponses()
    {
        // Arrange
        var projects = new List<Project>
        {
            new Project { Id = 1, Name = "P1", Description = "D1", Status = ProjectStatus.Active, Issues = new List<Issue> { new Issue(), new Issue() } },
            new Project { Id = 2, Name = "P2", Description = null, Status = ProjectStatus.Archived, Issues = new List<Issue>() }
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(projects);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new
        {
            Id = 1,
            Name = "P1",
            Description = "D1",
            Status = ProjectStatus.Active.ToString(),
            IssueCount = 2
        }, opt => opt.ExcludingMissingMembers());
        result[1].Should().BeEquivalentTo(new
        {
            Id = 2,
            Name = "P2",
            Description = (string?)null,
            Status = ProjectStatus.Archived.ToString(),
            IssueCount = 0
        }, opt => opt.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);

        // Act
        var result = await _sut.GetByIdAsync(123);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_AddsAndSaves_ReturnsResponse()
    {
        // Arrange
        var req = new CreateProjectRequest("New Project", "Desc");
        _repo.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project p, CancellationToken _) => p);

        // Act
        var res = await _sut.CreateAsync(req);

        // Assert
        res.Name.Should().Be("New Project");
        res.Description.Should().Be("Desc");
        res.Status.Should().Be(ProjectStatus.Active.ToString());
        res.IssueCount.Should().Be(0);

        _repo.Verify(r => r.AddAsync(It.Is<Project>(p =>
            p.Name == "New Project" &&
            p.Description == "Desc" &&
            p.Status == ProjectStatus.Active
        ), It.IsAny<CancellationToken>()), Times.Once);

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndSaves_ReturnsResponse()
    {
        // Arrange
        var existing = new Project { Id = 10, Name = "Old", Description = "OldD", Status = ProjectStatus.Active };
        _repo.Setup(r => r.GetByIdForUpdateAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var req = new UpdateProjectRequest("New", "NewD", ProjectStatus.Inactive);

        // Act
        var res = await _sut.UpdateAsync(10, req);

        // Assert
        res.Should().NotBeNull();
        res!.Name.Should().Be("New");
        res.Description.Should().Be("NewD");
        res.Status.Should().Be(ProjectStatus.Inactive.ToString());

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdForUpdateAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);

        // Act
        var res = await _sut.UpdateAsync(404, new UpdateProjectRequest(null, null, null));

        // Assert
        res.Should().BeNull();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFound_WhenEntityMissing()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdForUpdateAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);

        // Act
        var result = await _sut.DeleteAsync(999);

        // Assert
        result.Should().Be(DeleteProjectResult.NotFound);
        _repo.Verify(r => r.RemoveAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAndSaves_ReturnsDeleted()
    {
        // Arrange
        var existing = new Project { Id = 5, Name = "ToDelete" };
        _repo.Setup(r => r.GetByIdForUpdateAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        // Act
        var result = await _sut.DeleteAsync(5);

        // Assert
        result.Should().Be(DeleteProjectResult.Deleted);
        _repo.Verify(r => r.RemoveAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
