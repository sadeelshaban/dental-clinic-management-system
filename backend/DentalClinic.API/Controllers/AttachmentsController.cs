using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Attachments;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost("upload")]
    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] ulong? patientId,
        [FromForm] ulong? patientTreatmentId)
    {
        var dto = await _attachmentService.UploadAsync(file, patientId, patientTreatmentId, User);
        return Ok(ApiResponse<AttachmentDto>.Ok(dto));
    }

    [HttpGet("{id:long}/download")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> Download(ulong id)
    {
        var result = await _attachmentService.OpenDownloadAsync(id, User);
        if (result is null)
        {
            return NotFound(ApiResponse<object>.Fail("Attachment not found."));
        }

        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> ListByPatient(ulong patientId)
    {
        var list = await _attachmentService.ListByPatientAsync(patientId, User);
        return Ok(ApiResponse<object>.Ok(list));
    }

    [HttpGet("treatment/{patientTreatmentId}")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> ListByTreatment(ulong patientTreatmentId)
    {
        var list = await _attachmentService.ListByTreatmentAsync(patientTreatmentId, User);
        return Ok(ApiResponse<object>.Ok(list));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    public async Task<IActionResult> Delete(ulong id)
    {
        var deleted = await _attachmentService.DeleteAsync(id, User);
        return Ok(ApiResponse<object>.Ok(deleted));
    }
}
