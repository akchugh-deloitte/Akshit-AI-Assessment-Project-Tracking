using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApi.API.DTOs;
using ServiceApi.API.Services;
using ServiceApi.API.Utilities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IO;

namespace ServiceApi.API.Controllers;

[ApiController]
[Route("api/issues/{issueId:int}/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _service;
    public AttachmentsController(IAttachmentService service) => _service = service;


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

        // Basic validation: max size 10 MB and allowed content types/extensions
        const long maxBytes = 10 * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            var errors = new Dictionary<string, string[]> { ["file"] = new[] { "File size must be 10 MB or less." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".pdf")
        {
            var errors = new Dictionary<string, string[]> { ["file"] = new[] { "Unsupported file extension. Allowed: .jpg, .jpeg, .png, .pdf." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }

        if (file.ContentType != "image/jpeg" &&
            file.ContentType != "image/png" &&
            file.ContentType != "application/pdf")
        {
            var errors = new Dictionary<string, string[]> { ["file"] = new[] { "Unsupported content type. Allowed: image/jpeg, image/png, application/pdf." } };
            return ValidationProblem(new ValidationProblemDetails(errors) { Status = 400, Title = "Validation failed" });
        }

        var created = await _service.UploadAsync(issueId, User.GetUserId(), file);
        return CreatedAtAction(nameof(GetAll), new { issueId }, created);
    }

    /// <summary>Delete an attachment (uploader or Admin)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int issueId, int id)
    {
        var result = await _service.DeleteAsync(issueId, id, User.GetUserId(), User.IsInRole("Admin"));
        return result switch
        {
            DeleteAttachmentResult.NotFound => NotFound(),
            DeleteAttachmentResult.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

}
