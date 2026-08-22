using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Expense category management. Writes are ADMIN-only (enforced at the controller);
/// reads follow the expense module's read policy. Categories are soft-deactivated,
/// never deleted, because historical expenses reference them.
/// </summary>
public interface IExpenseCategoryService
{
    Task<PagedResult<ExpenseCategoryDto>> GetCategoriesAsync(
        ulong clinicId,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<ExpenseCategoryDto?> GetCategoryByIdAsync(
        ulong clinicId,
        ulong categoryId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Duplicate name within the clinic.</exception>
    Task<ExpenseCategoryDto> CreateCategoryAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateExpenseCategoryRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated category, or null when it does not exist in the clinic.</returns>
    Task<ExpenseCategoryDto?> UpdateCategoryAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong categoryId,
        UpdateExpenseCategoryRequest request,
        CancellationToken cancellationToken = default);
}