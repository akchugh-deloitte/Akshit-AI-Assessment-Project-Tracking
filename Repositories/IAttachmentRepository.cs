using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public interface IAttachmentRepository
{
    Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default);
    Task<List<Attachment>> GetByIssueIdAsync(int issueId, CancellationToken ct = default);
    Task<Attachment> AddAsync(Attachment attachment, CancellationToken ct = default);
    Task<Attachment?> GetByIdAsync(int issueId, int id, CancellationToken ct = default);
    Task RemoveAsync(Attachment attachment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
