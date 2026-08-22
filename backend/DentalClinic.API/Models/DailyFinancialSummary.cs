using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class DailyFinancialSummary
{
    public ulong ClinicId { get; set; }

    public DateOnly? FinancialDate { get; set; }

    public decimal? Revenue { get; set; }

    public decimal? Expenses { get; set; }

    public decimal? NetProfit { get; set; }
}
