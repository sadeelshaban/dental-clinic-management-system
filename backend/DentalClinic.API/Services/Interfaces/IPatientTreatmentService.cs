using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Patient treatments = treatments actually performed/recorded for a patient.
/// Name and unit price are SNAPSHOTTED at creation time; later catalog edits never
/// alter historical records. Status lifecycle (UNPAID/PARTIALLY_PAID/PAID/VOIDED)
/// is managed by the billing module (Phase 4) — Phase 3 only reads it and always
/// creates records as UNPAID.
/// </summary>
public interface IPatientTreatmentService
{
    /// <summary>Paged, DB-side filtered listing. DOCTOR actors are always scoped to their own profile.</summary>
    Task<PagedResult<PatientTreatmentListItemDto>> GetPatientTreatmentsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        PatientTreatmentSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <returns>The detail, or null when it does not exist in scope.</returns>
    Task<PatientTreatmentDetailDto?> GetPatientTreatmentByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientTreatmentId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Validation or ownership failures.</exception>
    Task<PatientTreatmentDetailDto> CreatePatientTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        CreatePatientTreatmentRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated detail, or null when it does not exist in scope.</returns>
    Task<PatientTreatmentDetailDto?> UpdatePatientTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientTreatmentId,
        UpdatePatientTreatmentRequest request,
        CancellationToken cancellationToken = default);
}