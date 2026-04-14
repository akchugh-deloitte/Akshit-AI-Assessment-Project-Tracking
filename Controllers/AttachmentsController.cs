using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApi.API.DTOs;
using ServiceApi.API.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ServiceApi.API.Controllers;

[ApiController]
[Route("api/issues/{issueId:int}/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _service;
    public AttachmentsController(IAttachmentService service) => _service = service;

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all attachments for an issue</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetAll(int issueId)
    {
        if (!await _service.IssueExistsAsync(issueId))
            return NotFound(new { message = "Issue not found" });

        var attachments = await _service.GetAllAsync(issueId);
        return Ok(attachments);
    }


    /// <summary>Upload a file for an issue (multipart/form-data)</summary>
    [HttpPost("upload")]
    public async Task<ActionResult<AttachmentResponse>> Upload(
        int issueId,
        [FromForm] IFormFile file)
    {
        if (!await _service.IssueExistsAsync(issueId))
            return NotFound(new { message = "Issue not found" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var created = await _service.UploadAsync(issueId, CurrentUserId, file);
        return CreatedAtAction(nameof(GetAll), new { issueId }, created);
    }

    /// <summary>Delete an attachment (uploader or Admin)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int issueId, int id)
    {
        var result = await _service.DeleteAsync(issueId, id, CurrentUserId, User.IsInRole("Admin"));
        return result switch
        {
            DeleteAttachmentResult.NotFound => NotFound(),
            DeleteAttachmentResult.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

}
