using System;
using System.Linq;
using System.Threading.Tasks;
using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Attachments;
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
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] ulong? patientId, [FromForm] ulong? patientTreatmentId)
    {
        // validate file
        if (file == null) return BadRequest(DentalClinic.API.DTOs.Common.ApiResponse<object>.Fail("File is required."));

        // size and mime checks
        var maxBytes = 10 * 1024 * 1024; // 10 MB
                if (file.Length > maxBytes) return BadRequest(DentalClinic.API.DTOs.Common.ApiResponse<object>.Fail("File exceeds the maximum allowed size (10 MB)."));

        var allowedTypes = new[] { "application/pdf" };
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == false && !allowedTypes.Contains(file.ContentType))
        {
                    return BadRequest(DentalClinic.API.DTOs.Common.ApiResponse<object>.Fail("File type not allowed."));
        }

        var dto = await _attachmentService.UploadAsync(file, patientId, patientTreatmentId, User);
                return Ok(DentalClinic.API.DTOs.Common.ApiResponse<AttachmentDto>.Ok(dto));
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> ListByPatient(ulong patientId)
    {
        var list = await _attachmentService.ListByPatientAsync(patientId, User);
            return Ok(DentalClinic.API.DTOs.Common.ApiResponse<object>.Ok(list));
    }

    [HttpGet("treatment/{patientTreatmentId}")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> ListByTreatment(ulong patientTreatmentId)
    {
        var list = await _attachmentService.ListByTreatmentAsync(patientTreatmentId, User);
            return Ok(DentalClinic.API.DTOs.Common.ApiResponse<object>.Ok(list));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    public async Task<IActionResult> Delete(ulong id)
    {
        var deleted = await _attachmentService.DeleteAsync(id, User);
            return Ok(DentalClinic.API.DTOs.Common.ApiResponse<object>.Ok(deleted));
    }
}
