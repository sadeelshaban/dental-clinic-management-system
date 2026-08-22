using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Expense
{
    public ulong ExpenseId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong? CategoryId { get; set; }

    public ulong? SupplierId { get; set; }

    public string ExpenseType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateOnly ExpenseDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ExpenseCategory? Category { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ExpensePayment> ExpensePayments { get; set; } = new List<ExpensePayment>();

    public virtual Supplier? Supplier { get; set; }
}
