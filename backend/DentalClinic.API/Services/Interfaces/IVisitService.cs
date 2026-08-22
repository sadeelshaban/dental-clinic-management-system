using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Services.Interfaces;

public interface IVisitService
{
    /// <summary>Paged, DB-side filtered listing. DOCTOR actors are always scoped to their own profile.</summary>
    Task<PagedResult<VisitListItemDto>> GetVisitsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        VisitSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <returns>The visit detail, or null when it does not exist in scope.</returns>
    Task<VisitDetailDto?> GetVisitByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong visitId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Validation or ownership failures.</exception>
    Task<VisitDetailDto> CreateVisitAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        CreateVisitRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated detail, or null when the visit does not exist in scope.</returns>
    Task<VisitDetailDto?> UpdateVisitAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong visitId,
        UpdateVisitRequest request,
        CancellationToken cancellationToken = default);
}