using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class ExpenseFinancial
{
    public ulong ExpenseId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong? SupplierId { get; set; }

    public ulong? CategoryId { get; set; }

    public string Description { get; set; } = null!;

    public string ExpenseType { get; set; } = null!;

    public DateOnly ExpenseDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? TotalPaid { get; set; }

    public decimal? RemainingBalance { get; set; }
}
