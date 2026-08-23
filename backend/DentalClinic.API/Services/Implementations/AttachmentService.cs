using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Attachments;
using DentalClinic.API.Extensions;
using DentalClinic.API.Models;
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
        AttachmentFileValidator.Validate(file);

        if (patientId == null && patientTreatmentId == null)
        {
            throw new BusinessRuleException("Either patientId or patientTreatmentId must be provided.");
        }

        var clinicId = user.GetClinicId();
        var actorUserId = user.GetUserId();

        if (patientId != null)
        {
            var exists = await _db.Patients.AnyAsync(p => p.PatientId == patientId && p.ClinicId == clinicId);
            if (!exists)
            {
                throw new BusinessRuleException("Patient not found or not in your clinic.");
            }
        }

        if (patientTreatmentId != null)
        {
            var exists = await _db.PatientTreatments.AnyAsync(
                t => t.PatientTreatmentId == patientTreatmentId && t.ClinicId == clinicId);
            if (!exists)
            {
                throw new BusinessRuleException("Patient treatment not found or not in your clinic.");
            }
        }

        var guid = Guid.NewGuid().ToString("N");
        var safeFileName = Path.GetFileName(file.FileName);
        var relativeDir = Path.Combine($"clinic_{clinicId}");
        var relativeName = Path.Combine(relativeDir, guid + "_" + safeFileName);

        await using var uploadStream = file.OpenReadStream();
        AttachmentFileValidator.ValidateContent(uploadStream);
        uploadStream.Position = 0;

        string fileUrl;
        try
        {
            _storage.EnsureStorageExists();
            fileUrl = await _storage.SaveAsync(relativeName, uploadStream);
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

        _auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.Attachment,
            attachment.AttachmentId,
            newData: new { attachment.FileName, attachment.FileSize, attachment.PatientId, attachment.PatientTreatmentId });

        await _db.SaveChangesAsync();

        return MapToDto(attachment);
    }

    public async Task<IEnumerable<AttachmentDto>> ListByPatientAsync(ulong patientId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();
        var list = await _db.Attachments
            .Where(a => a.PatientId == patientId && a.ClinicId == clinicId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return list.Select(MapToDto);
    }

    public async Task<IEnumerable<AttachmentDto>> ListByTreatmentAsync(ulong patientTreatmentId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();
        var list = await _db.Attachments
            .Where(a => a.PatientTreatmentId == patientTreatmentId && a.ClinicId == clinicId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return list.Select(MapToDto);
    }

    public async Task<AttachmentDownloadResult?> OpenDownloadAsync(ulong attachmentId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();

        var attachment = await _db.Attachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.ClinicId == clinicId);

        if (attachment is null)
        {
            return null;
        }

        var relativePath = ToStorageRelativePath(attachment.FileUrl);
        var stored = await _storage.OpenReadAsync(relativePath);
        if (stored is null)
        {
            return null;
        }

        var contentType = !string.IsNullOrWhiteSpace(attachment.FileType)
            ? attachment.FileType
            : stored.Value.ContentType;

        return new AttachmentDownloadResult
        {
            Content = stored.Value.Stream,
            ContentType = contentType,
            FileName = attachment.FileName
        };
    }

    public async Task<bool> DeleteAsync(ulong attachmentId, ClaimsPrincipal user)
    {
        var clinicId = user.GetClinicId();
        var actorUserId = user.GetUserId();

        var attachment = await _db.Attachments.FirstOrDefaultAsync(
            a => a.AttachmentId == attachmentId && a.ClinicId == clinicId);
        if (attachment == null)
        {
            throw new BusinessRuleException("Attachment not found.");
        }

        var role = user.GetRole();
        if (role != AppRoles.Admin && attachment.UploadedBy != actorUserId)
        {
            throw new BusinessRuleException("Not authorized to delete this attachment.");
        }

        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync();

        var relativePath = ToStorageRelativePath(attachment.FileUrl);
        var deleted = await _storage.DeleteAsync(relativePath);

        _auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Delete,
            AuditEntities.Attachment,
            attachment.AttachmentId,
            oldData: new { attachment.FileName, attachment.FileSize, attachment.PatientId, attachment.PatientTreatmentId });

        await _db.SaveChangesAsync();

        return deleted;
    }

    private static AttachmentDto MapToDto(Attachment attachment) =>
        new(
            attachment.AttachmentId,
            attachment.ClinicId,
            attachment.PatientId,
            attachment.PatientTreatmentId,
            attachment.FileName,
            $"/api/attachments/{attachment.AttachmentId}/download",
            attachment.FileType,
            attachment.FileSize,
            attachment.UploadedBy,
            attachment.CreatedAt);

    private static string ToStorageRelativePath(string fileUrl)
    {
        var relativePath = fileUrl.TrimStart('/');
        if (relativePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath["uploads/".Length..];
        }

        return relativePath;
    }
}
