using ServiceApi.API.DTOs;

namespace ServiceApi.API.Services;

public enum DeleteIssueResult
{
    NotFound,
    Deleted
}

public interface IIssueService
{
    // Existence checks (used by controllers for validation)
    Task<bool> ProjectExistsAsync(int projectId, CancellationToken ct = default);
    Task<bool> UserExistsAsync(int userId, CancellationToken ct = default);

    // Queries
    Task<IReadOnlyList<IssueResponse>> GetAllByProjectAsync(int projectId, IssueFilterRequest filter, CancellationToken ct = default);
    Task<IssueResponse?> GetByIdAsync(int projectId, int id, CancellationToken ct = default);
    Task<IReadOnlyList<IssueResponse>> GetAllGlobalAsync(IssueFilterRequest filter, CancellationToken ct = default);

    // Commands
    Task<IssueResponse> CreateAsync(int projectId, int reporterId, CreateIssueRequest request, CancellationToken ct = default);
    Task<IssueResponse?> UpdateAsync(int projectId, int id, UpdateIssueRequest request, CancellationToken ct = default);
    Task<DeleteIssueResult> DeleteAsync(int projectId, int id, CancellationToken ct = default);
}
