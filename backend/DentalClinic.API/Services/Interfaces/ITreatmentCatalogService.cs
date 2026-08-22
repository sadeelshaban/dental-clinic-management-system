using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Treatment categories + treatment catalog. Writes are ADMIN-only (enforced at the
/// controller); reads are available to all clinical staff. Catalog items are soft-
/// deactivated, never deleted, because historical patient treatments reference them.
/// </summary>
public interface ITreatmentCatalogService
{
    // Categories
    Task<PagedResult<TreatmentCategoryDto>> GetCategoriesAsync(
        ulong clinicId, TreatmentCategorySearchQuery query, CancellationToken cancellationToken = default);

    Task<TreatmentCategoryDto?> GetCategoryByIdAsync(
        ulong clinicId, ulong categoryId, CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Validation or duplicate-name failures.</exception>
    Task<TreatmentCategoryDto> CreateCategoryAsync(
        ulong clinicId, ulong actorUserId, CreateTreatmentCategoryRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated category, or null when it does not exist in the clinic.</returns>
    Task<TreatmentCategoryDto?> UpdateCategoryAsync(
        ulong clinicId, ulong actorUserId, ulong categoryId, UpdateTreatmentCategoryRequest request,
        CancellationToken cancellationToken = default);

    // Treatments (catalog)
    Task<PagedResult<TreatmentListItemDto>> GetTreatmentsAsync(
        ulong clinicId, TreatmentSearchQuery query, CancellationToken cancellationToken = default);

    Task<TreatmentDetailDto?> GetTreatmentByIdAsync(
        ulong clinicId, ulong treatmentId, CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Validation failures.</exception>
    Task<TreatmentDetailDto> CreateTreatmentAsync(
        ulong clinicId, ulong actorUserId, CreateTreatmentRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated treatment, or null when it does not exist in the clinic.</returns>
    Task<TreatmentDetailDto?> UpdateTreatmentAsync(
        ulong clinicId, ulong actorUserId, ulong treatmentId, UpdateTreatmentRequest request,
        CancellationToken cancellationToken = default);
}