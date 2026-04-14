using ServiceApi.API.Models;
using System.ComponentModel.DataAnnotations;

namespace ServiceApi.API.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

public record LoginResponse(string Token, string Username, string Role);

public record RegisterRequest(
    [Required][StringLength(50)] string Username,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string Role = "Member"
);

// ── Project ───────────────────────────────────────────────────────────────────
public record CreateProjectRequest(
    [Required][StringLength(100)] string Name,
    string? Description
);

public record UpdateProjectRequest(
    [StringLength(100)] string? Name,
    string? Description,
    ProjectStatus? Status
);

public record ProjectResponse(
    int Id,
    string Name,
    string? Description,
    string Status,
    DateTime CreatedOn,
    int IssueCount
);

// ── Issue ─────────────────────────────────────────────────────────────────────
public record CreateIssueRequest(
    [Required][StringLength(200)] string Title,
    string? Description,
    IssuePriority Priority = IssuePriority.Medium,
    int? AssigneeId = null,
    DateTime? DueDate = null
);

public record UpdateIssueRequest(
    [StringLength(200)] string? Title,
    string? Description,
    IssueStatus? Status,
    IssuePriority? Priority,
    int? AssigneeId,
    DateTime? DueDate
);

public record IssueResponse(
    int Id,
    int ProjectId,
    string ProjectName,
    string Title,
    string? Description,
    string Status,
    string Priority,
    int? AssigneeId,
    string? AssigneeName,
    int ReporterId,
    string ReporterName,
    DateTime? DueDate,
    DateTime CreatedOn,
    DateTime? UpdatedOn,
    int CommentCount
);

public record IssueFilterRequest
{
    public int? ProjectId { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public int? AssigneeId { get; init; }
    public string? Search { get; init; }  // title search
    public DateTime? DueBefore { get; init; }

    // Pagination (optional)
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }

    // Sorting (optional)
    // Allowed: createdOn, updatedOn, dueDate, priority, status, title, assigneeName
    public string? SortBy { get; init; }
    public string? SortDir { get; init; } // asc|desc
}

// ── Comment ───────────────────────────────────────────────────────────────────
public record CreateCommentRequest(
    [Required][StringLength(2000)] string Content
);

public record CommentResponse(
    int Id,
    int IssueId,
    int AuthorId,
    string AuthorName,
    string Content,
    DateTime CreatedOn
);

// ── Attachment ────────────────────────────────────────────────────────────────
public record CreateAttachmentRequest(
    [Required][StringLength(255)] string FileName,
    [Required] string FilePath,
    string? ContentType
);

public record AttachmentResponse(
    int Id,
    int IssueId,
    string FileName,
    string FilePath,
    string? ContentType,
    int UploadedById,
    string UploadedByName,
    DateTime UploadedOn
);
