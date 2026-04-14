using ServiceApi.API.DTOs;

namespace ServiceApi.API.Services;

public enum DeleteProjectResult
{
    NotFound,
    Deleted
}

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken ct = default);
    Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken ct = default);
    Task<DeleteProjectResult> DeleteAsync(int id, CancellationToken ct = default);
}
