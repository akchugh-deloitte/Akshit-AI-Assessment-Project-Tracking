using Microsoft.EntityFrameworkCore;
using ServiceApi.API.Data;
using ServiceApi.API.Models;

namespace ServiceApi.API.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;
    public ProjectRepository(AppDbContext db) => _db = db;

    public Task<List<Project>> GetAllAsync(CancellationToken ct = default) =>
        _db.Projects
            .Include(p => p.Issues)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Projects
            .Include(p => p.Issues)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Project?> GetByIdForUpdateAsync(int id, CancellationToken ct = default) =>
        _db.Projects
            .Include(p => p.Issues)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        await _db.Projects.AddAsync(project, ct);
        return project;
    }

    public Task RemoveAsync(Project project, CancellationToken ct = default)
    {
        _db.Projects.Remove(project);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
