using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Patient billing. Revenue = actual valid (non-voided) money received — never the
/// treatment value or outstanding balances. Treatment status is SERVER-DERIVED from
/// valid payments after every create/void; clients can never set it. Overpayments
/// are rejected. Payments are never deleted — they are voided with a reason and
/// remain stored/auditable while being excluded from all totals.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Records a payment against a patient treatment inside a transaction that locks
    /// the treatment row, so concurrent payments can never exceed the treatment total.
    /// </summary>
    /// <exception cref="Common.BusinessRuleException">Validation, overpayment, or concurrency failures.</exception>
    Task<PaymentDetailDto> CreatePaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Voids a payment (soft reversal). The payment remains stored but stops counting
    /// toward totals/revenue; the treatment status is recalculated.
    /// </summary>
    /// <returns>The voided payment detail, or null when the payment does not exist in the clinic.</returns>
    /// <exception cref="Common.BusinessRuleException">Already voided.</exception>
    Task<PaymentDetailDto?> VoidPaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong paymentId,
        VoidPaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Paged, DB-side filtered listing. DOCTOR actors see only payments on their own treatments.</summary>
    Task<PagedResult<PaymentListItemDto>> GetPaymentsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        PaymentSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <returns>The payment detail, or null when it does not exist in scope.</returns>
    Task<PaymentDetailDto?> GetPaymentByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong paymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patient financial statement built from the existing patient_financial_summary
    /// and patient_treatment_financials views. DOCTOR actors see only their own
    /// treatment lines/payments within the statement (safest pending-boundary choice).
    /// </summary>
    /// <returns>The statement, or null when the patient does not exist in the clinic.</returns>
    Task<PatientFinancialStatementDto?> GetPatientFinancialStatementAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientId,
        CancellationToken cancellationToken = default);
}