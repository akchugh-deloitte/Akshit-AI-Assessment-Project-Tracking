using ServiceApi.API.DTOs;
using ServiceApi.API.Models;
using ServiceApi.API.Repositories;

namespace ServiceApi.API.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;
    public CommentService(ICommentRepository repo) => _repo = repo;

    public Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default) =>
        _repo.IssueExistsAsync(issueId, ct);

    public async Task<IReadOnlyList<CommentResponse>> GetAllAsync(int issueId, CancellationToken ct = default)
    {
        var comments = await _repo.GetByIssueIdAsync(issueId, ct);
        return comments.Select(ToResponse).ToList();
    }

    public async Task<CommentResponse> CreateAsync(int issueId, int currentUserId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var comment = new Comment
        {
            IssueId = issueId,
            AuthorId = currentUserId,
            Content = request.Content,
            CreatedOn = DateTime.UtcNow
        };

        await _repo.AddAsync(comment, ct);
        await _repo.SaveChangesAsync(ct);

        return ToResponse(comment);
    }

    public async Task<DeleteCommentResult> DeleteAsync(int issueId, int id, int currentUserId, bool isAdmin, CancellationToken ct = default)
    {
        var comment = await _repo.GetByIdAsync(issueId, id, ct);
        if (comment == null) return DeleteCommentResult.NotFound;

        if (comment.AuthorId != currentUserId && !isAdmin)
            return DeleteCommentResult.Forbidden;

        await _repo.RemoveAsync(comment, ct);
        await _repo.SaveChangesAsync(ct);
        return DeleteCommentResult.Deleted;
    }

    private static CommentResponse ToResponse(Comment c) => new(
        c.Id,
        c.IssueId,
        c.AuthorId,
        c.Author?.Username ?? string.Empty,
        c.Content,
        c.CreatedOn
    );
}
