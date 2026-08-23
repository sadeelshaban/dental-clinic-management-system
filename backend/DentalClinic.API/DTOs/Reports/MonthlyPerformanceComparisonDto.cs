namespace DentalClinic.API.DTOs.Reports;

public record MonthlyPerformanceComparisonDto(
    string Month,
    decimal Revenue,
    decimal Expenses,
    decimal NetProfit,
    decimal OutstandingPatientBalances,
    long Patients,
    long Appointments,
    decimal? PreviousMonthRevenue,
    decimal? PreviousMonthExpenses,
    decimal? PreviousMonthProfit,
    long? PreviousMonthPatients,
    long? PreviousMonthAppointments,
    decimal? RevenueChangePercent,
    decimal? ExpenseChangePercent,
    decimal? ProfitChangePercent,
    decimal? PatientChangePercent,
    decimal? AppointmentChangePercent
);
