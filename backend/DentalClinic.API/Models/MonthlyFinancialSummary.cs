using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class MonthlyFinancialSummary
{
    public ulong ClinicId { get; set; }

    public string? Month { get; set; }

    public decimal? Revenue { get; set; }

    public decimal? Expenses { get; set; }

    public decimal? NetProfit { get; set; }

    public long? Patients { get; set; }

    public long? Appointments { get; set; }
}
