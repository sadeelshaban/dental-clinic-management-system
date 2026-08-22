using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class ExpensePayment
{
    public ulong ExpensePaymentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong ExpenseId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Method { get; set; } = null!;

    public ulong? PaymentMethodId { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public ulong? PaidBy { get; set; }

    public bool IsVoided { get; set; }

    public DateTime? VoidedAt { get; set; }

    public ulong? VoidedBy { get; set; }

    public string? VoidReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Expense Expense { get; set; } = null!;

    public virtual User? PaidByNavigation { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual User? VoidedByNavigation { get; set; }
}
