namespace ServiceApi.API.Models;

public enum IssueStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum IssuePriority
{
    Low,
    Medium,
    High,
    Critical
}

public class Issue
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public int? AssigneeId { get; set; }
    public int ReporterId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; }

    public Project Project { get; set; } = null!;
    public User? Assignee { get; set; }
    public User Reporter { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
