using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Clinical;

public class VisitListItemDto
{
    public ulong VisitId { get; set; }

    public ulong PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public ulong DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Clinic-local wall-clock date/time of the encounter (Asia/Gaza).</summary>
    public DateTime VisitDate { get; set; }

    public string? ChiefComplaint { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}

public class VisitDetailDto : VisitListItemDto
{
    public string? Diagnosis { get; set; }

    public string? ClinicalNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class CreateVisitRequest
{
    [Required]
    public ulong? PatientId { get; set; }

    /// <summary>Required for ADMIN. Ignored for DOCTOR users — the authenticated doctor's own profile is used.</summary>
    public ulong? DoctorId { get; set; }

    [Required]
    public DateTime? VisitDate { get; set; }

    [MaxLength(2000)]
    public string? ChiefComplaint { get; set; }

    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    public string? ClinicalNotes { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}

public class UpdateVisitRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    public ulong? DoctorId { get; set; }

    public DateTime? VisitDate { get; set; }

    [MaxLength(2000)]
    public string? ChiefComplaint { get; set; }

    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    public string? ClinicalNotes { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}

public class VisitSearchQuery
{
    public ulong? PatientId { get; set; }

    /// <summary>Ignored (overridden) for DOCTOR users, who are always scoped to their own profile.</summary>
    public ulong? DoctorId { get; set; }

    /// <summary>Exact-day filter on the visit's local date part.</summary>
    public DateOnly? Date { get; set; }

    /// <summary>Range start (inclusive), compared against the visit's local date part.</summary>
    public DateOnly? From { get; set; }

    /// <summary>Range end (inclusive).</summary>
    public DateOnly? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}