using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class MonthlyPerformanceComparison
{
    public ulong ClinicId { get; set; }

    public string? Month { get; set; }

    public decimal? Revenue { get; set; }

    public decimal? Expenses { get; set; }

    public decimal? NetProfit { get; set; }

    public long? Patients { get; set; }

    public long? Appointments { get; set; }

    public decimal? PreviousMonthRevenue { get; set; }

    public decimal? PreviousMonthExpenses { get; set; }

    public decimal? PreviousMonthProfit { get; set; }

    public long? PreviousMonthPatients { get; set; }

    public long? PreviousMonthAppointments { get; set; }

    public decimal? RevenueChangePercent { get; set; }

    public decimal? ExpenseChangePercent { get; set; }

    public decimal? ProfitChangePercent { get; set; }

    public decimal? PatientChangePercent { get; set; }

    public decimal? AppointmentChangePercent { get; set; }
}
