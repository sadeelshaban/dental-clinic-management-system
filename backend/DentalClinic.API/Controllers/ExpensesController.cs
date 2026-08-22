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
public class ExpensesController(IExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseListItemDto>>>> GetExpenses(
        [FromQuery] ExpenseSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await expenseService.GetExpensesAsync(User.GetClinicId(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpenseListItemDto>>.Ok(result));
    }

    [HttpGet("{expenseId:long}")]
    public async Task<ActionResult<ApiResponse<ExpenseDetailDto>>> GetExpense(
        ulong expenseId,
        CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetExpenseByIdAsync(User.GetClinicId(), expenseId, cancellationToken);
        if (expense is null)
        {
            return NotFound(ApiResponse<ExpenseDetailDto>.Fail("Expense not found."));
        }
                return Ok(ApiResponse<ExpenseDetailDto>.Ok(expense));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseDetailDto>>> CreateExpense(
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var expense = await expenseService.CreateExpenseAsync(User.GetClinicId(), User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(
            nameof(GetExpense),
            new { expenseId = expense.ExpenseId },
            ApiResponse<ExpenseDetailDto>.Ok(expense, "Expense created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPut("{expenseId:long}")]
    public async Task<ActionResult<ApiResponse<ExpenseDetailDto>>> UpdateExpense(
        ulong expenseId,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var expense = await expenseService.UpdateExpenseAsync(User.GetClinicId(), User.GetUserId(), expenseId, request, cancellationToken);
        if (expense is null)
        {
            return NotFound(ApiResponse<ExpenseDetailDto>.Fail("Expense not found."));
        }
                return Ok(ApiResponse<ExpenseDetailDto>.Ok(expense, "Expense updated successfully."));
    }
}