using Microsoft.EntityFrameworkCore;
using ServiceApi.API.Data;
using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly AppDbContext _db;
    public AttachmentRepository(AppDbContext db) => _db = db;

    public Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default) =>
        _db.Issues.AnyAsync(i => i.Id == issueId, ct);

    public Task<List<Attachment>> GetByIssueIdAsync(int issueId, CancellationToken ct = default) =>
        _db.Attachments
            .Include(a => a.UploadedBy)
            .Where(a => a.IssueId == issueId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Attachment> AddAsync(Attachment attachment, CancellationToken ct = default)
    {
        await _db.Attachments.AddAsync(attachment, ct);
        return attachment;
    }

    public Task<Attachment?> GetByIdAsync(int issueId, int id, CancellationToken ct = default) =>
        _db.Attachments.FirstOrDefaultAsync(a => a.Id == id && a.IssueId == issueId, ct);

    public Task RemoveAsync(Attachment attachment, CancellationToken ct = default)
    {
        _db.Attachments.Remove(attachment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
