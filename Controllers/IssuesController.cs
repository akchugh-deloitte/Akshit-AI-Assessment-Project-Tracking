using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApi.API.Services;
using ServiceApi.API.DTOs;
using ServiceApi.API.Utilities;

namespace ServiceApi.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/issues")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _service;

    public IssuesController(IIssueService service) => _service = service;


    /// <summary>List issues for a project with optional filters</summary>
    [ResponseCache(Duration = 15, Location = ResponseCacheLocation.Any, NoStore = false)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IssueResponse>>> GetAll(
        int projectId,
        [FromQuery] IssueFilterRequest filter)
    {
        if (!await _service.ProjectExistsAsync(projectId))
            return NotFound(new { message = "Project not found" });

        var issues = await _service.GetAllByProjectAsync(projectId, filter);
        return Ok(issues);
    }

    /// <summary>Get a specific issue</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<IssueResponse>> GetById(int projectId, int id)
    {
        var issue = await _service.GetByIdAsync(projectId, id);
        if (issue == null) return NotFound();
        return Ok(issue);
    }

    /// <summary>Create an issue under a project</summary>
    [HttpPost]
    public async Task<ActionResult<IssueResponse>> Create(
        int projectId,
        [FromBody] CreateIssueRequest request)
    {
        if (!await _service.ProjectExistsAsync(projectId))
            return NotFound(new { message = "Project not found" });

        if (request.AssigneeId.HasValue &&
            !await _service.UserExistsAsync(request.AssigneeId.Value))
            return BadRequest(new { message = "Assignee user not found" });

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            var errors = new Dictionary<string, string[]> { ["Title"] = new[] { "Title cannot be empty or whitespace." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            var errors = new Dictionary<string, string[]> { ["DueDate"] = new[] { "Due date cannot be in the past." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (!Enum.IsDefined(typeof(Models.IssuePriority), request.Priority))
        {
            var errors = new Dictionary<string, string[]> { ["Priority"] = new[] { "Invalid priority value." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (request.AssigneeId.HasValue && request.AssigneeId.Value < 0)
        {
            var errors = new Dictionary<string, string[]> { ["AssigneeId"] = new[] { "AssigneeId cannot be negative." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }

        var created = await _service.CreateAsync(projectId, User.GetUserId(), request);

        return CreatedAtAction(nameof(GetById),
            new { projectId, id = created.Id }, created);
    }

    /// <summary>Update an issue</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<IssueResponse>> Update(
        int projectId, int id,
        [FromBody] UpdateIssueRequest request)
    {
        if (request.AssigneeId.HasValue && request.AssigneeId != 0 &&
            !await _service.UserExistsAsync(request.AssigneeId.Value))
        {
            return BadRequest(new { message = "Assignee user not found" });
        }

        // Basic validation
        if (request.Title is not null && string.IsNullOrWhiteSpace(request.Title))
        {
            var errors = new Dictionary<string, string[]> { ["Title"] = new[] { "Title cannot be empty or whitespace." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            var errors = new Dictionary<string, string[]> { ["DueDate"] = new[] { "Due date cannot be in the past." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (request.Status.HasValue && !Enum.IsDefined(typeof(Models.IssueStatus), request.Status.Value))
        {
            var errors = new Dictionary<string, string[]> { ["Status"] = new[] { "Invalid status value." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (request.Priority.HasValue && !Enum.IsDefined(typeof(Models.IssuePriority), request.Priority.Value))
        {
            var errors = new Dictionary<string, string[]> { ["Priority"] = new[] { "Invalid priority value." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }
        if (request.AssigneeId.HasValue && request.AssigneeId.Value < 0)
        {
            var errors = new Dictionary<string, string[]> { ["AssigneeId"] = new[] { "AssigneeId cannot be negative." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }

        var updated = await _service.UpdateAsync(projectId, id, request);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    /// <summary>Delete an issue (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int projectId, int id)
    {
        var result = await _service.DeleteAsync(projectId, id);
        return result switch
        {
            DeleteIssueResult.NotFound => NotFound(),
            _ => NoContent()
        };
    }

}

/// <summary>Global issue listing across all projects (for dashboard-style queries)</summary>
[ApiController]
[Route("api/issues")]
[Authorize]
public class AllIssuesController : ControllerBase
{
    private readonly IIssueService _service;
    public AllIssuesController(IIssueService service) => _service = service;

    /// <summary>List all issues with filters (cross-project)</summary>
    [ResponseCache(Duration = 15, Location = ResponseCacheLocation.Any, NoStore = false)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IssueResponse>>> GetAll(
        [FromQuery] IssueFilterRequest filter)
    {
        var issues = await _service.GetAllGlobalAsync(filter);
        return Ok(issues);
    }
}
