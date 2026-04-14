using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApi.API.DTOs;
using ServiceApi.API.Services;

namespace ServiceApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service) => _service = service;

    /// <summary>List all projects</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAll(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var items = await _service.GetAllAsync();

        // Sorting
        var sort = (sortBy ?? string.Empty).ToLowerInvariant();
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        IEnumerable<ProjectResponse> query = items;

        query = sort switch
        {
            "createdon" => desc ? items.OrderByDescending(p => p.CreatedOn) : items.OrderBy(p => p.CreatedOn),
            "name" => desc ? items.OrderByDescending(p => p.Name) : items.OrderBy(p => p.Name),
            "status" => desc ? items.OrderByDescending(p => p.Status) : items.OrderBy(p => p.Status),
            "issuecount" => desc ? items.OrderByDescending(p => p.IssueCount) : items.OrderBy(p => p.IssueCount),
            _ => items.OrderByDescending(p => p.CreatedOn)
        };

        // Paging
        var page = pageNumber.GetValueOrDefault(1);
        if (page < 1) page = 1;

        var size = pageSize.GetValueOrDefault(20);
        if (size < 1) size = 1;
        if (size > 100) size = 100;

        var paged = query.Skip((page - 1) * size).Take(size).ToList();
        return Ok(paged);
    }

    /// <summary>Get a single project by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id)
    {
        var project = await _service.GetByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(project);
    }

    /// <summary>Create a new project (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectResponse>> Create([FromBody] CreateProjectRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update project details (Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectResponse>> Update(int id, [FromBody] UpdateProjectRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>Delete a project (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result switch
        {
            DeleteProjectResult.NotFound => NotFound(),
            _ => NoContent()
        };
    }

}
