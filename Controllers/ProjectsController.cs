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
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAll()
    {
        var projects = await _service.GetAllAsync();
        return Ok(projects);
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
