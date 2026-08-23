namespace DentalClinic.API.DTOs.Reports;

public record DailyFinancialSummaryDto(
    DateOnly FinancialDate,
    decimal Revenue,
    decimal Expenses,
    decimal NetProfit
);
