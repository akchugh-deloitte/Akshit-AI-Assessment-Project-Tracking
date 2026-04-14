using ServiceApi.API.Models;

namespace ServiceApi.API.Models;

public enum ProjectStatus
{
    Active,
    Inactive,
    Archived
}

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; }

    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
