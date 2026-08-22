using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Doctors;

namespace DentalClinic.API.Services.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorListItemDto>> GetDoctorsAsync(
        ulong clinicId,
        DoctorSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <returns>The doctor detail, or null when the doctor does not exist in the clinic.</returns>
    Task<DoctorDetailDto?> GetDoctorByIdAsync(
        ulong clinicId,
        ulong doctorId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates doctor profile fields (license number, specialization, bio).</summary>
    /// <returns>The updated detail, or null when the doctor does not exist in the clinic.</returns>
    Task<DoctorDetailDto?> UpdateDoctorAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong doctorId,
        UpdateDoctorRequest request,
        CancellationToken cancellationToken = default);
}