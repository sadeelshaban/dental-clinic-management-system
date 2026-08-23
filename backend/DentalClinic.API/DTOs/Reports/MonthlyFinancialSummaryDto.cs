namespace DentalClinic.API.DTOs.Reports;

public record MonthlyFinancialSummaryDto(
    string Month,
    decimal Revenue,
    decimal Expenses,
    decimal NetProfit,
    long Patients,
    long Appointments
);
