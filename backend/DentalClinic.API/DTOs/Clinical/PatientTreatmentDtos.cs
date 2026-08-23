using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Clinical;

public class PatientTreatmentListItemDto
{
    public ulong PatientTreatmentId { get; set; }

    public ulong PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public ulong DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public ulong? VisitId { get; set; }

    public ulong? TreatmentId { get; set; }

    /// <summary>Snapshot of the treatment name at recording time (catalog edits never change it).</summary>
    public string TreatmentName { get; set; } = string.Empty;

    /// <summary>Clinic-local wall-clock date/time (Asia/Gaza).</summary>
    public DateTime TreatmentDate { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Snapshot of the charged unit price at recording time.</summary>
    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    /// <summary>Database-generated: GREATEST(quantity × unit_price − discount, 0).</summary>
    public decimal FinalAmount { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class PatientTreatmentDetailDto : PatientTreatmentListItemDto
{
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class CreatePatientTreatmentRequest
{
    [Required]
    public ulong? PatientId { get; set; }

    /// <summary>Required for ADMIN. Ignored for DOCTOR users — the authenticated doctor's own profile is used.</summary>
    public ulong? DoctorId { get; set; }

    /// <summary>Optional link to a visit; must belong to the same clinic AND same patient.</summary>
    public ulong? VisitId { get; set; }

    /// <summary>Catalog item. When provided, name and price are SNAPSHOTTED from it.
    /// When omitted, TreatmentName and UnitPrice must be supplied (custom/ad-hoc treatment).</summary>
    public ulong? TreatmentId { get; set; }

    /// <summary>Required only when TreatmentId is omitted.</summary>
    [MaxLength(200)]
    public string? TreatmentName { get; set; }

    /// <summary>Defaults to now (clinic-local).</summary>
    public DateTime? TreatmentDate { get; set; }

    /// <summary>Defaults to 1. Must be > 0.</summary>
    [Range(0.01, 999999999)]
    public decimal? Quantity { get; set; }

    /// <summary>Defaults to the catalog default price when a catalog item is used. Must be >= 0.</summary>
    [Range(0, 999999999)]
    public decimal? UnitPrice { get; set; }

    /// <summary>Defaults to 0. Must be greater than or equal to 0 and less than or equal to quantity multiplied by unit_price.</summary>
    [Range(0, 999999999)]
    public decimal? DiscountAmount { get; set; }

    public string? Notes { get; set; }
}

public class UpdatePatientTreatmentRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.
    /// TreatmentId and TreatmentName are intentionally NOT updatable (historical record integrity).</summary>
    [Range(0.01, 999999999)]
    public decimal? Quantity { get; set; }

    [Range(0, 999999999)]
    public decimal? UnitPrice { get; set; }

    [Range(0, 999999999)]
    public decimal? DiscountAmount { get; set; }

    public string? Notes { get; set; }

    /// <summary>Re-link to another visit; must belong to the same clinic and patient.</summary>
    public ulong? VisitId { get; set; }

    public DateTime? TreatmentDate { get; set; }
}

public class PatientTreatmentSearchQuery
{
    public ulong? PatientId { get; set; }

    /// <summary>Ignored (overridden) for DOCTOR users, who are always scoped to their own profile.</summary>
    public ulong? DoctorId { get; set; }

    public ulong? VisitId { get; set; }

    public ulong? TreatmentId { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    /// <summary>Exact status filter: UNPAID, PARTIALLY_PAID, PAID, VOIDED.</summary>
    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}