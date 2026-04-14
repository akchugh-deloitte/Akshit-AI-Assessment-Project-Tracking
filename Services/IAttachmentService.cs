using ServiceApi.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace ServiceApi.API.Services;

public enum DeleteAttachmentResult
{
    NotFound,
    Forbidden,
    Deleted
}

public interface IAttachmentService
{
    Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default);
    Task<IReadOnlyList<AttachmentResponse>> GetAllAsync(int issueId, CancellationToken ct = default);
    Task<AttachmentResponse> CreateAsync(int issueId, int currentUserId, CreateAttachmentRequest request, CancellationToken ct = default);
    Task<DeleteAttachmentResult> DeleteAsync(int issueId, int id, int currentUserId, bool isAdmin, CancellationToken ct = default);
    Task<AttachmentResponse> UploadAsync(int issueId, int currentUserId, IFormFile file, CancellationToken ct = default);
}
