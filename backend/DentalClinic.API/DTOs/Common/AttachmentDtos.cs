using System;

namespace DentalClinic.API.DTOs.Attachments
{
    public record AttachmentDto(
        ulong AttachmentId,
        ulong ClinicId,
        ulong? PatientId,
        ulong? PatientTreatmentId,
        string FileName,
        string FileUrl,
        string? FileType,
        ulong? FileSize,
        ulong? UploadedBy,
        DateTime CreatedAt
    );

    public record CreateAttachmentResponseDto(
        ulong AttachmentId,
        string FileUrl
    );
}
