using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Attachment
{
    public ulong AttachmentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong? PatientId { get; set; }

    public ulong? PatientTreatmentId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public ulong? FileSize { get; set; }

    public ulong? UploadedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Patient? Patient { get; set; }

    public virtual PatientTreatment? PatientTreatment { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
