using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Clinic expenses (obligations) + expense payments. Mirrors the Phase 4 patient
/// billing model on the payable side: an expense's total is the OBLIGATION and is
/// NOT treated as paid at creation; status is SERVER-DERIVED from valid payments
/// after every create/void; overpayments are rejected; payments are never deleted
/// — they are voided with a reason and remain stored/auditable while being
/// excluded from all totals. Concurrency: the expense row is locked FOR UPDATE
/// inside the transaction during payment create/void.
/// </summary>
public interface IExpenseService
{
    // Expenses
    Task<PagedResult<ExpenseListItemDto>> GetExpensesAsync(
        ulong clinicId,
        ExpenseSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<ExpenseDetailDto?> GetExpenseByIdAsync(
        ulong clinicId,
        ulong expenseId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Validation failures.</exception>
    Task<ExpenseDetailDto> CreateExpenseAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated expense, or null when it does not exist in the clinic.</returns>
    Task<ExpenseDetailDto?> UpdateExpenseAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong expenseId,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default);

    // Expense payments
    /// <exception cref="Common.BusinessRuleException">Validation, overpayment, or concurrency failures.</exception>
    Task<ExpensePaymentDetailDto> CreateExpensePaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateExpensePaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The voided payment detail, or null when the payment does not exist in the clinic.</returns>
    /// <exception cref="Common.BusinessRuleException">Already voided.</exception>
    Task<ExpensePaymentDetailDto?> VoidExpensePaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong expensePaymentId,
        VoidExpensePaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ExpensePaymentListItemDto>> GetExpensePaymentsAsync(
        ulong clinicId,
        ExpensePaymentSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<ExpensePaymentDetailDto?> GetExpensePaymentByIdAsync(
        ulong clinicId,
        ulong expensePaymentId,
        CancellationToken cancellationToken = default);
}