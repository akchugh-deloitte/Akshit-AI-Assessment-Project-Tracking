using ServiceApi.API.DTOs;
using ServiceApi.API.Models;
using ServiceApi.API.Repositories;

namespace ServiceApi.API.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repo;
    public ProjectService(IProjectRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ProjectResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var projects = await _repo.GetAllAsync(ct);
        return projects.Select(ToResponse).ToList();
    }

    public async Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _repo.GetByIdAsync(id, ct);
        return project is null ? null : ToResponse(project);
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Status = ProjectStatus.Active,
            CreatedOn = DateTime.UtcNow
        };

        await _repo.AddAsync(project, ct);
        await _repo.SaveChangesAsync(ct);
        return ToResponse(project);
    }

    public async Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var project = await _repo.GetByIdForUpdateAsync(id, ct);
        if (project is null) return null;

        if (request.Name is not null) project.Name = request.Name;
        if (request.Description is not null) project.Description = request.Description;
        if (request.Status.HasValue) project.Status = request.Status.Value;
        project.UpdatedOn = DateTime.UtcNow;

        await _repo.SaveChangesAsync(ct);
        return ToResponse(project);
    }

    public async Task<DeleteProjectResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await _repo.GetByIdForUpdateAsync(id, ct);
        if (project is null) return DeleteProjectResult.NotFound;

        await _repo.RemoveAsync(project, ct);
        await _repo.SaveChangesAsync(ct);
        return DeleteProjectResult.Deleted;
    }

    private static ProjectResponse ToResponse(Project p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Status.ToString(),
        p.CreatedOn,
        p.Issues?.Count ?? 0
    );
}
