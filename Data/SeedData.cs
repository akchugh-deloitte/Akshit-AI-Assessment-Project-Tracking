using Microsoft.EntityFrameworkCore;
using ServiceApi.API.Data;
using ServiceApi.API.Models;

namespace ServiceApi.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // EnsureCreated creates the schema directly from the EF model.
        // Simpler and more reliable than MigrateAsync for SQLite in dev/assessment.
        // Trade-off: doesn't use migration history — fine for this scope.
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        var admin = new User
        {
            Username = "admin",
            Email = "admin@worktrack.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            CreatedOn = DateTime.UtcNow
        };

        var member = new User
        {
            Username = "member1",
            Email = "member1@worktrack.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
            Role = "Member",
            CreatedOn = DateTime.UtcNow
        };

        context.Users.AddRange(admin, member);
        await context.SaveChangesAsync();

        // Sample project + issues
        var project = new Project
        {
            Name = "WorkTrack Demo",
            Description = "Sample project seeded on startup",
            Status = ProjectStatus.Active,
            CreatedOn = DateTime.UtcNow
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var issue1 = new Issue
        {
            ProjectId = project.Id,
            Title = "Setup CI/CD pipeline",
            Description = "Configure GitHub Actions for automated builds",
            Status = IssueStatus.Open,
            Priority = IssuePriority.High,
            ReporterId = admin.Id,
            AssigneeId = member.Id,
            CreatedOn = DateTime.UtcNow
        };
        var issue2 = new Issue
        {
            ProjectId = project.Id,
            Title = "Write unit tests",
            Description = "Add coverage for core services",
            Status = IssueStatus.InProgress,
            Priority = IssuePriority.Medium,
            ReporterId = member.Id,
            CreatedOn = DateTime.UtcNow
        };
        context.Issues.AddRange(issue1, issue2);
        await context.SaveChangesAsync();

        context.Comments.Add(new Comment
        {
            IssueId = issue1.Id,
            AuthorId = admin.Id,
            Content = "Assigned to member1 for initial setup",
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}