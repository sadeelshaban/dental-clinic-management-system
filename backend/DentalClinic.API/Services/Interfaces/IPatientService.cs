using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Patients;

namespace DentalClinic.API.Services.Interfaces;

public interface IPatientService
{
    Task<PagedResult<PatientListItemDto>> GetPatientsAsync(
        ulong clinicId,
        PatientSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<PatientDetailDto?> GetPatientByIdAsync(
        ulong clinicId,
        ulong patientId,
        CancellationToken cancellationToken = default);

    Task<PatientDetailDto> CreatePatientAsync(
        ulong clinicId,
        ulong actorUserId,
        CreatePatientRequest request,
        CancellationToken cancellationToken = default);

    Task<PatientDetailDto?> UpdatePatientAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong patientId,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivatePatientAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong patientId,
        CancellationToken cancellationToken = default);
}
