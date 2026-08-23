using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DentalClinic.API.DTOs.Attachments;

namespace DentalClinic.API.Services.Interfaces;

public interface IAttachmentService
{
    Task<AttachmentDto> UploadAsync(IFormFile file, ulong? patientId, ulong? patientTreatmentId, ClaimsPrincipal user);
    Task<IEnumerable<AttachmentDto>> ListByPatientAsync(ulong patientId, ClaimsPrincipal user);
    Task<IEnumerable<AttachmentDto>> ListByTreatmentAsync(ulong patientTreatmentId, ClaimsPrincipal user);
    Task<bool> DeleteAsync(ulong attachmentId, ClaimsPrincipal user);

    /// <summary>
    /// Opens an attachment for download after clinic-scoped authorization checks.
    /// Caller must dispose the returned result.
    /// </summary>
    Task<AttachmentDownloadResult?> OpenDownloadAsync(ulong attachmentId, ClaimsPrincipal user);
}
