using DentalClinic.API.DTOs.Appointments;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Services.Interfaces;

public interface IAppointmentService
{
    /// <summary>
    /// Paged, DB-side filtered listing. DOCTOR actors are always scoped to their own
    /// doctor profile regardless of the query's DoctorId.
    /// </summary>
    Task<PagedResult<AppointmentListItemDto>> GetAppointmentsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        AppointmentSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <returns>The appointment detail, or null when it does not exist in scope
    /// (wrong clinic, or another doctor's appointment for a DOCTOR actor).</returns>
    Task<AppointmentDetailDto?> GetAppointmentByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong appointmentId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Validation, working-hours, slot, overlap, or ownership failures.</exception>
    Task<AppointmentDetailDto> CreateAppointmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated detail, or null when the appointment does not exist in scope.</returns>
    /// <exception cref="Common.BusinessRuleException">Validation, working-hours, slot, overlap, or ownership failures.</exception>
    Task<AppointmentDetailDto?> UpdateAppointmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong appointmentId,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Transitions: SCHEDULED→CONFIRMED, CONFIRMED→COMPLETED.</summary>
    Task<AppointmentDetailDto?> ConfirmAppointmentAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Transitions: SCHEDULED→COMPLETED, CONFIRMED→COMPLETED.</summary>
    Task<AppointmentDetailDto?> CompleteAppointmentAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Transitions: SCHEDULED→CANCELLED, CONFIRMED→CANCELLED.</summary>
    Task<AppointmentDetailDto?> CancelAppointmentAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Transitions: SCHEDULED→NO_SHOW, CONFIRMED→NO_SHOW.</summary>
    Task<AppointmentDetailDto?> MarkNoShowAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default);
}