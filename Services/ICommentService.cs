using ServiceApi.API.DTOs;

namespace ServiceApi.API.Services;

public enum DeleteCommentResult
{
    NotFound,
    Forbidden,
    Deleted
}

public interface ICommentService
{
    Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default);
    Task<IReadOnlyList<CommentResponse>> GetAllAsync(int issueId, CancellationToken ct = default);
    Task<CommentResponse> CreateAsync(int issueId, int currentUserId, CreateCommentRequest request, CancellationToken ct = default);
    Task<DeleteCommentResult> DeleteAsync(int issueId, int id, int currentUserId, bool isAdmin, CancellationToken ct = default);
}
