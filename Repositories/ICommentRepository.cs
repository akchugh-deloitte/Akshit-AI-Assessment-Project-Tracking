using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public interface ICommentRepository
{
    Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default);
    Task<List<Comment>> GetByIssueIdAsync(int issueId, CancellationToken ct = default);
    Task<Comment> AddAsync(Comment comment, CancellationToken ct = default);
    Task<Comment?> GetByIdAsync(int issueId, int id, CancellationToken ct = default);
    Task RemoveAsync(Comment comment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
