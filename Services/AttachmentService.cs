using ServiceApi.API.DTOs;
using ServiceApi.API.Models;
using ServiceApi.API.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ServiceApi.API.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AttachmentService> _logger;
    public AttachmentService(IAttachmentRepository repo, IWebHostEnvironment env, ILogger<AttachmentService> logger)
    {
        _repo = repo;
        _env = env;
        _logger = logger;
    }

    public Task<bool> IssueExistsAsync(int issueId, CancellationToken ct = default) =>
        _repo.IssueExistsAsync(issueId, ct);

    public async Task<IReadOnlyList<AttachmentResponse>> GetAllAsync(int issueId, CancellationToken ct = default)
    {
        var attachments = await _repo.GetByIssueIdAsync(issueId, ct);
        return attachments.Select(ToResponse).ToList();
    }

    public async Task<AttachmentResponse> CreateAsync(int issueId, int currentUserId, CreateAttachmentRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating attachment metadata for issue {IssueId} by user {UserId}: {FileName}", issueId, currentUserId, request.FileName);

        var attachment = new Attachment
        {
            IssueId = issueId,
            FileName = request.FileName,
            FilePath = request.FilePath,
            ContentType = request.ContentType,
            UploadedById = currentUserId,
            UploadedOn = DateTime.UtcNow
        };

        await _repo.AddAsync(attachment, ct);
        await _repo.SaveChangesAsync(ct);

        return ToResponse(attachment);
    }

    public async Task<DeleteAttachmentResult> DeleteAsync(int issueId, int id, int currentUserId, bool isAdmin, CancellationToken ct = default)
    {
        var attachment = await _repo.GetByIdAsync(issueId, id, ct);
        if (attachment == null) return DeleteAttachmentResult.NotFound;

        if (attachment.UploadedById != currentUserId && !isAdmin)
            return DeleteAttachmentResult.Forbidden;

        _logger.LogInformation("Deleting attachment {AttachmentId} for issue {IssueId} by user {UserId} (isAdmin={IsAdmin})", id, issueId, currentUserId, isAdmin);

        await _repo.RemoveAsync(attachment, ct);
        await _repo.SaveChangesAsync(ct);
        return DeleteAttachmentResult.Deleted;
    }

    public async Task<AttachmentResponse> UploadAsync(int issueId, int currentUserId, IFormFile file, CancellationToken ct = default)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadRoot = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadRoot);

        var originalName = Path.GetFileName(file.FileName);
        var safeName = string.Concat(originalName.Split(Path.GetInvalidFileNameChars()));
        var storedName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(uploadRoot, storedName);

        _logger.LogInformation("Uploading file for issue {IssueId} by user {UserId}: {Original} -> {Stored}", issueId, currentUserId, originalName, storedName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var attachment = new Attachment
        {
            IssueId = issueId,
            FileName = originalName,
            FilePath = $"/uploads/{storedName}",
            ContentType = file.ContentType,
            UploadedById = currentUserId,
            UploadedOn = DateTime.UtcNow
        };

        try
        {
            await _repo.AddAsync(attachment, ct);
            await _repo.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist attachment metadata. Rolling back stored file at {Path}", fullPath);
            try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { /* swallow cleanup errors */ }
            throw;
        }

        return ToResponse(attachment);
    }

    private static AttachmentResponse ToResponse(Attachment a) => new(
        a.Id,
        a.IssueId,
        a.FileName,
        a.FilePath,
        a.ContentType,
        a.UploadedById,
        a.UploadedBy?.Username ?? string.Empty,
        a.UploadedOn
    );
}
