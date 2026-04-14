using ServiceApi.API.DTOs;
using ServiceApi.API.Models;
using ServiceApi.API.Repositories;

namespace ServiceApi.API.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _repo;
    public IssueService(IIssueRepository repo) => _repo = repo;

    // Existence checks
    public Task<bool> ProjectExistsAsync(int projectId, CancellationToken ct = default) =>
        _repo.ProjectExistsAsync(projectId, ct);

    public Task<bool> UserExistsAsync(int userId, CancellationToken ct = default) =>
        _repo.UserExistsAsync(userId, ct);

    // Queries
    public async Task<IReadOnlyList<IssueResponse>> GetAllByProjectAsync(int projectId, IssueFilterRequest filter, CancellationToken ct = default)
    {
        var items = await _repo.GetAllByProjectAsync(projectId, filter, ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<IssueResponse?> GetByIdAsync(int projectId, int id, CancellationToken ct = default)
    {
        var issue = await _repo.GetByIdAsync(projectId, id, ct);
        return issue is null ? null : ToResponse(issue);
    }

    public async Task<IReadOnlyList<IssueResponse>> GetAllGlobalAsync(IssueFilterRequest filter, CancellationToken ct = default)
    {
        var items = await _repo.GetAllGlobalAsync(filter, ct);
        return items.Select(ToResponse).ToList();
    }

    // Commands
    public async Task<IssueResponse> CreateAsync(int projectId, int reporterId, CreateIssueRequest request, CancellationToken ct = default)
    {
        var issue = new Issue
        {
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            AssigneeId = request.AssigneeId,
            ReporterId = reporterId,
            DueDate = request.DueDate,
            Status = IssueStatus.Open,
            CreatedOn = DateTime.UtcNow
        };

        await _repo.AddAsync(issue, ct);
        await _repo.SaveChangesAsync(ct);
        return ToResponse(issue);
    }

    public async Task<IssueResponse?> UpdateAsync(int projectId, int id, UpdateIssueRequest request, CancellationToken ct = default)
    {
        var issue = await _repo.GetByIdForUpdateAsync(projectId, id, ct);
        if (issue is null) return null;

        if (request.Title is not null) issue.Title = request.Title;
        if (request.Description is not null) issue.Description = request.Description;
        if (request.Status.HasValue) issue.Status = request.Status.Value;
        if (request.Priority.HasValue) issue.Priority = request.Priority.Value;
        if (request.DueDate.HasValue) issue.DueDate = request.DueDate;

        if (request.AssigneeId.HasValue)
        {
            if (request.AssigneeId == 0)
                issue.AssigneeId = null;
            else
                issue.AssigneeId = request.AssigneeId;
        }

        issue.UpdatedOn = DateTime.UtcNow;
        await _repo.SaveChangesAsync(ct);
        return ToResponse(issue);
    }

    public async Task<DeleteIssueResult> DeleteAsync(int projectId, int id, CancellationToken ct = default)
    {
        var issue = await _repo.GetByIdForUpdateAsync(projectId, id, ct);
        if (issue is null) return DeleteIssueResult.NotFound;

        await _repo.RemoveAsync(issue, ct);
        await _repo.SaveChangesAsync(ct);
        return DeleteIssueResult.Deleted;
    }

    private static IssueResponse ToResponse(Issue i) => new(
        i.Id,
        i.ProjectId,
        i.Project?.Name ?? string.Empty,
        i.Title,
        i.Description,
        i.Status.ToString(),
        i.Priority.ToString(),
        i.AssigneeId,
        i.Assignee?.Username,
        i.ReporterId,
        i.Reporter?.Username ?? string.Empty,
        i.DueDate,
        i.CreatedOn,
        i.UpdatedOn,
        i.Comments?.Count ?? 0
    );
}
