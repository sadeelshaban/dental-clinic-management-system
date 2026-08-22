using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Clinic payment methods (Cash, Card, Bank Transfer, ...). Reads: all clinical
/// staff. Writes: ADMIN only (clinic configuration). Methods are clinic-scoped and
/// soft-deactivated — never deleted — because historical payments reference them.
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController(IPaymentMethodService paymentMethodService) : ControllerBase
{
    /// <summary>Optional filter: isActive=true/false; omit for all.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentMethodDto>>>> GetPaymentMethods(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await paymentMethodService.GetPaymentMethodsAsync(
            User.GetClinicId(), isActive, cancellationToken);
        return Ok(ApiResponse<PagedResult<PaymentMethodDto>>.Ok(result));
    }

    [HttpGet("{paymentMethodId:long}")]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> GetPaymentMethod(
        ulong paymentMethodId,
        CancellationToken cancellationToken)
    {
        var method = await paymentMethodService.GetPaymentMethodByIdAsync(
            User.GetClinicId(), paymentMethodId, cancellationToken);

        if (method is null)
        {
            return NotFound(ApiResponse<PaymentMethodDto>.Fail("Payment method not found."));
        }

        return Ok(ApiResponse<PaymentMethodDto>.Ok(method));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var method = await paymentMethodService.CreatePaymentMethodAsync(
            User.GetClinicId(), User.GetUserId(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetPaymentMethod),
            new { paymentMethodId = method.PaymentMethodId },
            ApiResponse<PaymentMethodDto>.Ok(method, "Payment method created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPut("{paymentMethodId:long}")]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> UpdatePaymentMethod(
        ulong paymentMethodId,
        [FromBody] UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var method = await paymentMethodService.UpdatePaymentMethodAsync(
            User.GetClinicId(), User.GetUserId(), paymentMethodId, request, cancellationToken);

        if (method is null)
        {
            return NotFound(ApiResponse<PaymentMethodDto>.Fail("Payment method not found."));
        }

        return Ok(ApiResponse<PaymentMethodDto>.Ok(method, "Payment method updated successfully."));
    }
}