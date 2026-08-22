using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Clinic-configurable payment methods (e.g., Cash, Card, Bank Transfer). Reads for
/// all clinical staff; writes ADMIN-only. Methods are clinic-scoped and soft-
/// deactivated — never deleted, because historical payments reference them.
/// </summary>
public interface IPaymentMethodService
{
    Task<PagedResult<PaymentMethodDto>> GetPaymentMethodsAsync(
        ulong clinicId,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(
        ulong clinicId,
        ulong paymentMethodId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Duplicate name within the clinic.</exception>
    Task<PaymentMethodDto> CreatePaymentMethodAsync(
        ulong clinicId,
        ulong actorUserId,
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated method, or null when it does not exist in the clinic.</returns>
    Task<PaymentMethodDto?> UpdatePaymentMethodAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong paymentMethodId,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken = default);
}