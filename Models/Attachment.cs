namespace ServiceApi.API.Models;

public class Attachment
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public int UploadedById { get; set; }
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

    public Issue Issue { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
