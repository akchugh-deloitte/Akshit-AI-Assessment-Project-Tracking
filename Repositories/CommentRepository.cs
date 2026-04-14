using Microsoft.EntityFrameworkCore;
using ServiceApi.API.Data;
using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _db;
    public CommentRepository(AppDbContext db) => _db = db;

    public Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default) =>
        _db.Issues.AnyAsync(i => i.Id == issueId, ct);

    public Task<List<Comment>> GetByIssueIdAsync(int issueId, CancellationToken ct = default) =>
        _db.Comments
            .Include(c => c.Author)
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedOn)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Comment> AddAsync(Comment comment, CancellationToken ct = default)
    {
        await _db.Comments.AddAsync(comment, ct);
        return comment;
    }

    public Task<Comment?> GetByIdAsync(int issueId, int id, CancellationToken ct = default) =>
        _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.IssueId == issueId, ct);

    public Task RemoveAsync(Comment comment, CancellationToken ct = default)
    {
        _db.Comments.Remove(comment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
