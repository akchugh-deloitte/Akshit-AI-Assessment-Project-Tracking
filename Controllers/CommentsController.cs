using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApi.API.DTOs;
using ServiceApi.API.Services;
using ServiceApi.API.Utilities;

namespace ServiceApi.API.Controllers;

[ApiController]
[Route("api/issues/{issueId:int}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _service;
    public CommentsController(ICommentService service) => _service = service;


    /// <summary>Get all comments for an issue</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetAll(int issueId)
    {
        if (!await _service.IssueExistsAsync(issueId))
            return NotFound(new { message = "Issue not found" });

        var comments = await _service.GetAllAsync(issueId);
        return Ok(comments);
    }

    /// <summary>Add a comment to an issue</summary>
    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Create(
        int issueId,
        [FromBody] CreateCommentRequest request)
    {
        if (!await _service.IssueExistsAsync(issueId))
            return NotFound(new { message = "Issue not found" });

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            var errors = new Dictionary<string, string[]> { ["Content"] = new[] { "Content cannot be empty or whitespace." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }

        var created = await _service.CreateAsync(issueId, User.GetUserId(), request);
        return CreatedAtAction(nameof(GetAll), new { issueId }, created);
    }

    /// <summary>Delete a comment (author or Admin)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int issueId, int id)
    {
        var result = await _service.DeleteAsync(issueId, id, User.GetUserId(), User.IsInRole("Admin"));
        return result switch
        {
            DeleteCommentResult.NotFound => NotFound(),
            DeleteCommentResult.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

}
