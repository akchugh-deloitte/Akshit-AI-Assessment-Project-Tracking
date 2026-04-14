using Microsoft.EntityFrameworkCore;
using ServiceApi.API.Data;
using ServiceApi.API.DTOs;
using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _db;
    public IssueRepository(AppDbContext db) => _db = db;

    public Task<bool> ProjectExistsAsync(int projectId, CancellationToken ct = default) =>
        _db.Projects.AnyAsync(p => p.Id == projectId, ct);

    public Task<bool> UserExistsAsync(int userId, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Id == userId, ct);

    public async Task<List<Issue>> GetAllByProjectAsync(int projectId, IssueFilterRequest filter, CancellationToken ct = default)
    {
        var query = _db.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Include(i => i.Project)
            .Include(i => i.Comments)
            .Where(i => i.ProjectId == projectId)
            .AsNoTracking()
            .AsQueryable();

        ApplyFilters(ref query, filter);
        return await query.OrderByDescending(i => i.CreatedOn).ToListAsync(ct);
    }

    public Task<Issue?> GetByIdAsync(int projectId, int id, CancellationToken ct = default) =>
        _db.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Include(i => i.Project)
            .Include(i => i.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.ProjectId == projectId, ct);

    public Task<Issue?> GetByIdForUpdateAsync(int projectId, int id, CancellationToken ct = default) =>
        _db.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Include(i => i.Project)
            .Include(i => i.Comments)
            .FirstOrDefaultAsync(i => i.Id == id && i.ProjectId == projectId, ct);

    public async Task<List<Issue>> GetAllGlobalAsync(IssueFilterRequest filter, CancellationToken ct = default)
    {
        var query = _db.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Include(i => i.Project)
            .Include(i => i.Comments)
            .AsNoTracking()
            .AsQueryable();

        if (filter.ProjectId.HasValue)
            query = query.Where(i => i.ProjectId == filter.ProjectId);

        ApplyFilters(ref query, filter);
        return await query.OrderByDescending(i => i.CreatedOn).ToListAsync(ct);
    }

    public async Task<Issue> AddAsync(Issue issue, CancellationToken ct = default)
    {
        await _db.Issues.AddAsync(issue, ct);
        return issue;
    }

    public Task RemoveAsync(Issue issue, CancellationToken ct = default)
    {
        _db.Issues.Remove(issue);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    private static void ApplyFilters(ref IQueryable<Issue> query, IssueFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<IssueStatus>(filter.Status, true, out var status))
            query = query.Where(i => i.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.Priority) &&
            Enum.TryParse<IssuePriority>(filter.Priority, true, out var priority))
            query = query.Where(i => i.Priority == priority);

        if (filter.AssigneeId.HasValue)
            query = query.Where(i => i.AssigneeId == filter.AssigneeId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(i => i.Title.Contains(filter.Search));

        if (filter.DueBefore.HasValue)
            query = query.Where(i => i.DueDate <= filter.DueBefore);
    }
}
