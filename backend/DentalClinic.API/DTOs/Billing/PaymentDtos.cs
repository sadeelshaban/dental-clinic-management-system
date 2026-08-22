using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Billing;

// ---------------------------------------------------------------------------
// Payments
// ---------------------------------------------------------------------------

public class PaymentListItemDto
{
    public ulong PaymentId { get; set; }

    public ulong PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public ulong PatientTreatmentId { get; set; }

    /// <summary>Snapshot of the treatment name at recording time.</summary>
    public string TreatmentName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Clinic-local wall-clock date/time (Asia/Gaza).</summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>One of: CASH, CARD, BANK_TRANSFER, CHEQUE, OTHER.</summary>
    public string Method { get; set; } = string.Empty;

    public ulong? PaymentMethodId { get; set; }

    public string? ReferenceNumber { get; set; }

    public bool IsVoided { get; set; }
}

public class PaymentDetailDto : PaymentListItemDto
{
    public string? Notes { get; set; }

    public string? VoidReason { get; set; }

    public DateTime? VoidedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentRequest
{
    [Required]
    public ulong? PatientTreatmentId { get; set; }

    /// <summary>Must be > 0 and <= remaining balance. Overpayments are rejected.</summary>
    [Required]
    [Range(0.01, 999999999)]
    public decimal? Amount { get; set; }

    /// <summary>One of: CASH, CARD, BANK_TRANSFER, CHEQUE, OTHER. Defaults to CASH.</summary>
    [MaxLength(20)]
    public string Method { get; set; } = "CASH";

    /// <summary>Optional clinic payment-method reference (must belong to current clinic and be active).</summary>
    public ulong? PaymentMethodId { get; set; }

    /// <summary>Defaults to now (clinic-local).</summary>
    public DateTime? PaymentDate { get; set; }

    [MaxLength(150)]
    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }
}

public class VoidPaymentRequest
{
    /// <summary>Required. Recorded for audit; voided payments stay stored but stop counting toward totals/revenue.</summary>
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class PaymentSearchQuery
{
    public ulong? PatientId { get; set; }

    public ulong? PatientTreatmentId { get; set; }

    /// <summary>Exact method filter: CASH, CARD, BANK_TRANSFER, CHEQUE, OTHER.</summary>
    public string? Method { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    /// <summary>Filter by voided state; omit to return both.</summary>
    public bool? IsVoided { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

// ---------------------------------------------------------------------------
// Payment methods (clinic-configurable)
// ---------------------------------------------------------------------------

public class PaymentMethodDto
{
    public ulong PaymentMethodId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class CreatePaymentMethodRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdatePaymentMethodRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    [MaxLength(100)]
    public string? Name { get; set; }

    public bool? IsActive { get; set; }
}

// ---------------------------------------------------------------------------
// Patient financial statement
// ---------------------------------------------------------------------------

public class StatementLineDto
{
    public ulong PatientTreatmentId { get; set; }

    /// <summary>Clinic-local date/time of the treatment.</summary>
    public DateTime TreatmentDate { get; set; }

    public string TreatmentName { get; set; } = string.Empty;

    public ulong DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public decimal TreatmentTotal { get; set; }

    public decimal Paid { get; set; }

    public decimal Remaining { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class PatientFinancialStatementDto
{
    public ulong PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientNumber { get; set; } = string.Empty;

    /// <summary>SUM of all treatment totals (NOT revenue).</summary>
    public decimal TotalTreatments { get; set; }

    /// <summary>SUM of valid (non-voided) payments = actual money received.</summary>
    public decimal TotalPaid { get; set; }

    /// <summary>TotalTreatments − TotalPaid = outstanding balance (NOT revenue).</summary>
    public decimal TotalRemaining { get; set; }

    public IReadOnlyList<StatementLineDto> Lines { get; init; } = [];

    public IReadOnlyList<PaymentListItemDto> Payments { get; init; } = [];
}