using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Patient billing. Writes (create/void): ADMIN + SECRETARY per the confirmed
/// permission matrix (SECRETARY records payments). Reads: all clinical staff, with
/// DOCTOR actors scoped to payments on their own treatments.
///
/// Financial rules enforced server-side (clients can never set status/remaining/
/// revenue): revenue = valid (non-voided) money received; overpayments rejected;
/// status derived after every create/void; payments are never deleted — voiding
/// keeps them stored/auditable while excluding them from all totals.
/// Concurrency: treatment row is locked FOR UPDATE inside the transaction.
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentListItemDto>>>> GetPayments(
        [FromQuery] PaymentSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.GetPaymentsAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PaymentListItemDto>>.Ok(result));
    }

    [HttpGet("{paymentId:long}")]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> GetPayment(
        ulong paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetPaymentByIdAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), paymentId, cancellationToken);

        if (payment is null)
        {
            return NotFound(ApiResponse<PaymentDetailDto>.Fail("Payment not found."));
        }

        return Ok(ApiResponse<PaymentDetailDto>.Ok(payment));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.CreatePaymentAsync(
            User.GetClinicId(), User.GetUserId(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetPayment),
            new { paymentId = payment.PaymentId },
            ApiResponse<PaymentDetailDto>.Ok(payment, "Payment recorded successfully."));
    }

    /// <summary>Soft-reversal: the payment stays stored/auditable but stops counting toward totals and revenue.</summary>
    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPost("{paymentId:long}/void")]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> VoidPayment(
        ulong paymentId,
        [FromBody] VoidPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.VoidPaymentAsync(
            User.GetClinicId(), User.GetUserId(), paymentId, request, cancellationToken);

        if (payment is null)
        {
            return NotFound(ApiResponse<PaymentDetailDto>.Fail("Payment not found."));
        }

        return Ok(ApiResponse<PaymentDetailDto>.Ok(payment, "Payment voided successfully."));
    }
}