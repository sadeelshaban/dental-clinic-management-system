using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Expenses;

// ---------------------------------------------------------------------------
// Expense categories
// ---------------------------------------------------------------------------

public class ExpenseCategoryDto
{
    public ulong CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseCategoryRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateExpenseCategoryRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    [MaxLength(150)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    /// <summary>Soft deactivation preferred over deletion (categories may be referenced historically).</summary>
    public bool? IsActive { get; set; }
}

// ---------------------------------------------------------------------------
// Suppliers
// ---------------------------------------------------------------------------

public class SupplierDto
{
    public ulong SupplierId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? ContactPerson { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateSupplierRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    public string? Notes { get; set; }
}

public class UpdateSupplierRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    public string? Notes { get; set; }

    /// <summary>Soft deactivation preferred over deletion (expenses reference suppliers).</summary>
    public bool? IsActive { get; set; }
}

public class SupplierSearchQuery
{
    /// <summary>Matches supplier name or contact person.</summary>
    public string? Search { get; set; }

    public bool? IsActive { get; set; } = true;
}

// ---------------------------------------------------------------------------
// Expenses (obligations)
// ---------------------------------------------------------------------------

public class ExpenseListItemDto
{
    public ulong ExpenseId { get; set; }

    public ulong? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public ulong? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    /// <summary>One of the schema ENUM values: GENERAL, SUPPLIER_PURCHASE, RENT, UTILITIES, EQUIPMENT, MAINTENANCE, LABORATORY, MATERIALS, OTHER.</summary>
    public string ExpenseType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Clinic-local date (Asia/Gaza).</summary>
    public DateOnly ExpenseDate { get; set; }

    public DateOnly? DueDate { get; set; }

    /// <summary>Total obligation — NOT money actually paid.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Server-derived from valid expense payments: UNPAID / PARTIALLY_PAID / PAID.</summary>
    public string Status { get; set; } = string.Empty;
}

public class ExpenseDetailDto : ExpenseListItemDto
{
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class CreateExpenseRequest
{
    /// <summary>Optional; must belong to the current clinic when provided.</summary>
    public ulong? SupplierId { get; set; }

    /// <summary>Optional; must belong to the current clinic when provided.</summary>
    public ulong? CategoryId { get; set; }

    /// <summary>Schema ENUM: GENERAL, SUPPLIER_PURCHASE, RENT, UTILITIES, EQUIPMENT, MAINTENANCE, LABORATORY, MATERIALS, OTHER. Defaults to GENERAL.</summary>
    [MaxLength(30)]
    public string ExpenseType { get; set; } = "GENERAL";

    [Required]
    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Clinic-local date; defaults to today.</summary>
    public DateOnly? ExpenseDate { get; set; }

    public DateOnly? DueDate { get; set; }

    /// <summary>Total obligation. Must be > 0. NOT treated as paid on creation.</summary>
    [Required]
    [Range(0.01, 999999999)]
    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }
}

public class UpdateExpenseRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.
    /// Status is server-derived and never client-settable.</summary>
    public ulong? SupplierId { get; set; }

    public ulong? CategoryId { get; set; }

    [MaxLength(30)]
    public string? ExpenseType { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    public DateOnly? ExpenseDate { get; set; }

    public DateOnly? DueDate { get; set; }

    /// <summary>Changing the total re-derives status; payments already recorded must not exceed it.</summary>
    [Range(0.01, 999999999)]
    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }
}

public class ExpenseSearchQuery
{
    public ulong? SupplierId { get; set; }

    public ulong? CategoryId { get; set; }

    /// <summary>Exact type filter (schema ENUM values).</summary>
    public string? ExpenseType { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    /// <summary>Exact derived-status filter: UNPAID, PARTIALLY_PAID, PAID.</summary>
    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

// ---------------------------------------------------------------------------
// Expense payments
// ---------------------------------------------------------------------------

public class ExpensePaymentListItemDto
{
    public ulong ExpensePaymentId { get; set; }

    public ulong ExpenseId { get; set; }

    public string? SupplierName { get; set; }

    public decimal Amount { get; set; }

    /// <summary>Clinic-local wall-clock date/time (Asia/Gaza).</summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>One of: CASH, CARD, BANK_TRANSFER, CHEQUE, OTHER.</summary>
    public string Method { get; set; } = string.Empty;

    public ulong? PaymentMethodId { get; set; }

    public string? ReferenceNumber { get; set; }

    public bool IsVoided { get; set; }
}

public class ExpensePaymentDetailDto : ExpensePaymentListItemDto
{
    public string? Notes { get; set; }

    public string? VoidReason { get; set; }

    public DateTime? VoidedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateExpensePaymentRequest
{
    [Required]
    public ulong? ExpenseId { get; set; }

    /// <summary>Must be greater than 0 and less than or equal to remaining balance. Overpayments are rejected.</summary>
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

public class VoidExpensePaymentRequest
{
    /// <summary>Required. Voided payments stay stored but stop counting toward totals.</summary>
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class ExpensePaymentSearchQuery
{
    public ulong? ExpenseId { get; set; }

    public ulong? SupplierId { get; set; }

    /// <summary>Exact method filter: CASH, CARD, BANK_TRANSFER, CHEQUE, OTHER.</summary>
    public string? Method { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public bool? IsVoided { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

// ---------------------------------------------------------------------------
// Supplier financial statement
// ---------------------------------------------------------------------------

public class SupplierStatementLineDto
{
    public ulong ExpenseId { get; set; }

    /// <summary>Clinic-local date (Asia/Gaza).</summary>
    public DateOnly ExpenseDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public string ExpenseType { get; set; } = string.Empty;

    public string? CategoryName { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal Paid { get; set; }

    public decimal Remaining { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class SupplierFinancialStatementDto
{
    public ulong SupplierId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    /// <summary>Count of expenses linked to this supplier (from supplier_financial_summary view).</summary>
    public long TotalTransactions { get; set; }

    /// <summary>SUM of expense totals for this supplier (obligations — NOT cash outflow).</summary>
    public decimal TotalPurchases { get; set; }

    /// <summary>SUM of valid (non-voided) expense payments = actual money paid out.</summary>
    public decimal TotalPaid { get; set; }

    /// <summary>Outstanding balance owed to this supplier (NOT cash outflow).</summary>
    public decimal TotalRemaining { get; set; }

    public IReadOnlyList<SupplierStatementLineDto> Lines { get; init; } = [];

    public IReadOnlyList<ExpensePaymentListItemDto> Payments { get; init; } = [];
}