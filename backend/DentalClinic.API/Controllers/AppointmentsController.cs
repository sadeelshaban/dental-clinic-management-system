using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Appointments;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Appointment scheduling. All clinic scoping comes from the JWT; DOCTOR actors are
/// always scoped to their own doctor profile (client-supplied doctorId is ignored
/// for them). ADMIN/SECRETARY manage the whole clinic; DOCTOR manages own appointments.
///
/// Scheduling rules enforced in the service layer:
/// - start < end (no zero-duration)
/// - within the clinic's working hours for that weekday; closed days rejected
/// - start aligned to the 'appointment_slot_minutes' setting grid (when configured)
/// - overlap prevention against SCHEDULED/CONFIRMED appointments of the same doctor
///   (CANCELLED / NO_SHOW / COMPLETED never block), with a doctor-row lock inside a
///   transaction to serialize concurrent bookings.
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    /// <summary>
    /// Paged listing with filters: date (day view), from/to (week/range view),
    /// doctor, patient, status. Ordered by date then start time.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AppointmentListItemDto>>>> GetAppointments(
        [FromQuery] AppointmentSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetAppointmentsAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<AppointmentListItemDto>>.Ok(result));
    }

    [HttpGet("{appointmentId:long}")]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> GetAppointment(
        ulong appointmentId,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetAppointmentByIdAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), appointmentId, cancellationToken);

        if (appointment is null)
        {
            return NotFound(ApiResponse<AppointmentDetailDto>.Fail("Appointment not found."));
        }

        return Ok(ApiResponse<AppointmentDetailDto>.Ok(appointment));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> CreateAppointment(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.CreateAppointmentAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetAppointment),
            new { appointmentId = appointment.AppointmentId },
            ApiResponse<AppointmentDetailDto>.Ok(appointment, "Appointment created successfully."));
    }

    [HttpPut("{appointmentId:long}")]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> UpdateAppointment(
        ulong appointmentId,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.UpdateAppointmentAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), appointmentId, request, cancellationToken);

        if (appointment is null)
        {
            return NotFound(ApiResponse<AppointmentDetailDto>.Fail("Appointment not found."));
        }

        return Ok(ApiResponse<AppointmentDetailDto>.Ok(appointment, "Appointment updated successfully."));
    }

    /// <summary>SCHEDULED → CONFIRMED.</summary>
    [HttpPost("{appointmentId:long}/confirm")]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> ConfirmAppointment(
        ulong appointmentId,
        CancellationToken cancellationToken) =>
        await ChangeStatus(
            appointmentId,
            id => appointmentService.ConfirmAppointmentAsync(
                User.GetClinicId(), User.GetUserId(), User.GetRole(), id, cancellationToken),
            cancellationToken);

    /// <summary>SCHEDULED or CONFIRMED → COMPLETED.</summary>
    [HttpPost("{appointmentId:long}/complete")]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> CompleteAppointment(
        ulong appointmentId,
        CancellationToken cancellationToken) =>
        await ChangeStatus(
            appointmentId,
            id => appointmentService.CompleteAppointmentAsync(
                User.GetClinicId(), User.GetUserId(), User.GetRole(), id, cancellationToken),
            cancellationToken);

    /// <summary>SCHEDULED or CONFIRMED → CANCELLED. Cancelled appointments free their time slot.</summary>
    [HttpPost("{appointmentId:long}/cancel")]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> CancelAppointment(
        ulong appointmentId,
        CancellationToken cancellationToken) =>
        await ChangeStatus(
            appointmentId,
            id => appointmentService.CancelAppointmentAsync(
                User.GetClinicId(), User.GetUserId(), User.GetRole(), id, cancellationToken),
            cancellationToken);

    /// <summary>SCHEDULED or CONFIRMED → NO_SHOW. No-show appointments free their time slot.</summary>
    [HttpPost("{appointmentId:long}/no-show")]
    public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> MarkNoShow(
        ulong appointmentId,
        CancellationToken cancellationToken) =>
        await ChangeStatus(
            appointmentId,
            id => appointmentService.MarkNoShowAsync(
                User.GetClinicId(), User.GetUserId(), User.GetRole(), id, cancellationToken),
            cancellationToken);

    private async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> ChangeStatus(
        ulong appointmentId,
        Func<ulong, Task<AppointmentDetailDto?>> action,
        CancellationToken cancellationToken)
    {
        var appointment = await action(appointmentId);

        if (appointment is null)
        {
            return NotFound(ApiResponse<AppointmentDetailDto>.Fail("Appointment not found."));
        }

        return Ok(ApiResponse<AppointmentDetailDto>.Ok(appointment, "Appointment status updated successfully."));
    }
}