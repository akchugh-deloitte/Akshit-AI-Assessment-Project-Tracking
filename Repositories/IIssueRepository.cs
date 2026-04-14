using ServiceApi.API.DTOs;
using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public interface IIssueRepository
{
    // Existence checks
    Task<bool> ProjectExistsAsync(int projectId, CancellationToken ct = default);
    Task<bool> UserExistsAsync(int userId, CancellationToken ct = default);

    // Queries
    Task<List<Issue>> GetAllByProjectAsync(int projectId, IssueFilterRequest filter, CancellationToken ct = default);
    Task<Issue?> GetByIdAsync(int projectId, int id, CancellationToken ct = default);
    Task<Issue?> GetByIdForUpdateAsync(int projectId, int id, CancellationToken ct = default);
    Task<List<Issue>> GetAllGlobalAsync(IssueFilterRequest filter, CancellationToken ct = default);

    // Commands
    Task<Issue> AddAsync(Issue issue, CancellationToken ct = default);
    Task RemoveAsync(Issue issue, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
