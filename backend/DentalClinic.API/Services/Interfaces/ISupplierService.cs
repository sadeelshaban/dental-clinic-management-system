using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Supplier management + supplier financial statement. Writes are ADMIN-only
/// (enforced at the controller). Suppliers are soft-deactivated, never deleted,
/// because historical expenses reference them. The financial statement uses the
/// existing supplier_financial_summary and expense_financials views as
/// authoritative sources.
/// </summary>
public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetSuppliersAsync(
        ulong clinicId,
        SupplierSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<SupplierDto?> GetSupplierByIdAsync(
        ulong clinicId,
        ulong supplierId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Duplicate name within the clinic.</exception>
    Task<SupplierDto> CreateSupplierAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated supplier, or null when it does not exist in the clinic.</returns>
    Task<SupplierDto?> UpdateSupplierAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong supplierId,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supplier financial statement: totals from supplier_financial_summary view,
    /// per-expense lines from expense_financials view, plus payment history.
    /// </summary>
    /// <returns>The statement, or null when the supplier does not exist in the clinic.</returns>
    Task<SupplierFinancialStatementDto?> GetSupplierFinancialStatementAsync(
        ulong clinicId,
        ulong supplierId,
        CancellationToken cancellationToken = default);
}