namespace DentalClinic.API.DTOs.Reports;

public record DailyFinancialSummaryDto(
    DateOnly FinancialDate,
    decimal Revenue,
    decimal Expenses,
    decimal NetProfit
);

public record DailyFinancialReportDto(
    decimal OutstandingPatientBalances,
    IReadOnlyList<DailyFinancialSummaryDto> Items
);
