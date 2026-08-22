using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class SupplierFinancialSummary
{
    public ulong SupplierId { get; set; }

    public ulong ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public long TotalTransactions { get; set; }

    public decimal? TotalPurchases { get; set; }

    public decimal? TotalPaid { get; set; }

    public decimal? TotalRemaining { get; set; }
}
