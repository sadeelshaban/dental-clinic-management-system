using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class ExpensePaymentsController(IExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpensePaymentListItemDto>>>> GetExpensePayments(
        [FromQuery] ExpensePaymentSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await expenseService.GetExpensePaymentsAsync(User.GetClinicId(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpensePaymentListItemDto>>.Ok(result));
    }

    [HttpGet("{expensePaymentId:long}")]
    public async Task<ActionResult<ApiResponse<ExpensePaymentDetailDto>>> GetExpensePayment(
        ulong expensePaymentId,
        CancellationToken cancellationToken)
    {
        var p = await expenseService.GetExpensePaymentByIdAsync(User.GetClinicId(), expensePaymentId, cancellationToken);
        if (p is null)
        {
            return NotFound(ApiResponse<ExpensePaymentDetailDto>.Fail("Expense payment not found."));
        }
                return Ok(ApiResponse<ExpensePaymentDetailDto>.Ok(p));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpensePaymentDetailDto>>> CreateExpensePayment(
        [FromBody] CreateExpensePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await expenseService.CreateExpensePaymentAsync(User.GetClinicId(), User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(
            nameof(GetExpensePayment),
            new { expensePaymentId = payment.ExpensePaymentId },
            ApiResponse<ExpensePaymentDetailDto>.Ok(payment, "Expense payment recorded successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPost("{expensePaymentId:long}/void")]
    public async Task<ActionResult<ApiResponse<ExpensePaymentDetailDto>>> VoidExpensePayment(
        ulong expensePaymentId,
        [FromBody] VoidExpensePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await expenseService.VoidExpensePaymentAsync(User.GetClinicId(), User.GetUserId(), expensePaymentId, request, cancellationToken);
        if (payment is null)
        {
            return NotFound(ApiResponse<ExpensePaymentDetailDto>.Fail("Expense payment not found."));
        }
                return Ok(ApiResponse<ExpensePaymentDetailDto>.Ok(payment, "Expense payment voided successfully."));
    }
}