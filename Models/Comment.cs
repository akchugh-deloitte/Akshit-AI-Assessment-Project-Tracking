namespace ServiceApi.API.Models;

public class Comment
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Issue Issue { get; set; } = null!;
    public User Author { get; set; } = null!;
}
