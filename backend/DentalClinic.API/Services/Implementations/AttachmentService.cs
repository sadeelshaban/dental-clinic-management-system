using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DentalClinic.API.Common;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Attachments;
using DentalClinic.API.Models;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DentalClinic.API.Services.Implementations;

public class AttachmentService : IAttachmentService
{
    private readonly DentalClinicDbContext _db;
    private readonly IFileStorage _storage;
    private readonly IAuditService _auditService;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        DentalClinicDbContext db,
        IFileStorage storage,
        IAuditService auditService,
        ILogger<AttachmentService> logger)
    {
        _db = db;
        _storage = storage;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AttachmentDto> UploadAsync(IFormFile file, ulong? patientId, ulong? patientTreatmentId, ClaimsPrincipal user)
    {
        if (file == null) throw new BusinessRuleException("File is required.");
        if (patientId == null && patientTreatmentId == null) throw new BusinessRuleException("Either patientId or patientTreatmentId must be provided.");

        var clinicId = user.GetClinicId();
        var actorUserId = user.GetUserId();

        // ownership checks
        if (patientId != null)
        {
            var exists = await _db.Patients.AnyAsync(p => p.PatientId == patientId && p.ClinicId == clinicId);
            if (!exists) throw new BusinessRuleException("Patient not found or not in your clinic.");
        }

        if (patientTreatmentId != null)
        {
            var exists = await _db.PatientTreatments.AnyAsync(t => t.PatientTreatmentId == patientTreatmentId && t.ClinicId == clinicId);
            if (!exists) throw new BusinessRuleException("Patient treatment not found or not in your clinic.");
        }

        // validations: size and mime types enforced at controller; assume pre-checked here
        var guid = Guid.NewGuid().ToString("N");
        var safeFileName = Path.GetFileName(file.FileName);
        var ext = Path.GetExtension(safeFileName);
        var relativeDir = Path.Combine($"clinic_{clinicId}");
        var relativeName = Path.Combine(relativeDir, guid + "_" + safeFileName);

        // save to storage
        using var stream = file.OpenReadStream();
        string fileUrl;
        try
        {
            _storage.EnsureStorageExists();
            fileUrl = await _storage.SaveAsync(relativeName, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attachment file");
            throw new BusinessRuleException("Failed to save file.");
        }

        var attachment = new Attachment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            PatientTreatmentId = patientTreatmentId,
            FileName = safeFileName,
            FileUrl = fileUrl,
            FileType = file.ContentType,
            FileSize = (ulong?)file.Length,
            UploadedBy = actorUserId
        };

        await _db.Attachments.AddAsync(attachment);
        await _db.SaveChangesAsync();

        // audit
        _auditService.Record(actorUserId, clinicId, Constants.AuditActions.Create, Constants.AuditEntities.Attachment, attachment.AttachmentId, newData: new { attachment.FileName, attachment.FileUrl, attachment.FileSize });
        await _db.SaveChangesAsync();

        return new AttachmentDto(
            attachment.AttachmentId,
            attachment.ClinicId,
            attachment.PatientId,
            attachment.PatientTreatmentId,
            attachment.FileName,
            attachment.FileUrl,
            attachment.FileType,
            attachment.FileSize,
            attachment.UploadedBy,
            attachment.CreatedAt
        );
    }

    public async Task<IEnumerable<AttachmentDto>> ListByPatientAsync(ulong patientId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();
        var list = await _db.Attachments
            .Where(a => a.PatientId == patientId && a.ClinicId == clinicId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return list.Select(a => new AttachmentDto(a.AttachmentId, a.ClinicId, a.PatientId, a.PatientTreatmentId, a.FileName, a.FileUrl, a.FileType, a.FileSize, a.UploadedBy, a.CreatedAt));
    }

    public async Task<IEnumerable<AttachmentDto>> ListByTreatmentAsync(ulong patientTreatmentId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();
        var list = await _db.Attachments
            .Where(a => a.PatientTreatmentId == patientTreatmentId && a.ClinicId == clinicId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return list.Select(a => new AttachmentDto(a.AttachmentId, a.ClinicId, a.PatientId, a.PatientTreatmentId, a.FileName, a.FileUrl, a.FileType, a.FileSize, a.UploadedBy, a.CreatedAt));
    }

    public async Task<bool> DeleteAsync(ulong attachmentId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();
        var actorUserId = user.GetUserId();

        var attachment = await _db.Attachments.FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.ClinicId == clinicId);
        if (attachment == null) throw new BusinessRuleException("Attachment not found.");

        // delete permission: ADMIN or uploader
        var role = user.GetRole();
        if (role != Constants.AppRoles.Admin && attachment.UploadedBy != actorUserId)
            throw new BusinessRuleException("Not authorized to delete this attachment.");

        // attempt DB delete and file delete
        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync();

        // file path: derive relative path from FileUrl
        var fileUrl = attachment.FileUrl.TrimStart('/');
                // fileUrl expected like "uploads/clinic_{id}/{guid_filename}"
                var relativePath = fileUrl;
                if (relativePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath.Substring("uploads/".Length);
                }

                var deleted = await _storage.DeleteAsync(relativePath);

        _auditService.Record(actorUserId, clinicId, Constants.AuditActions.Delete, Constants.AuditEntities.Attachment, attachment.AttachmentId, oldData: new { attachment.FileName, attachment.FileUrl });
        await _db.SaveChangesAsync();

        return deleted;
    }
}
