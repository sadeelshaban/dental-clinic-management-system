using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Appointments;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class AppointmentService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IAppointmentService
{
    private const string StatusScheduled = "SCHEDULED";
    private const string StatusConfirmed = "CONFIRMED";
    private const string StatusCompleted = "COMPLETED";
    private const string StatusCancelled = "CANCELLED";
    private const string StatusNoShow = "NO_SHOW";

    /// <summary>
    /// Allowed status transitions. COMPLETED, CANCELLED and NO_SHOW are terminal.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> AllowedTransitions =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [StatusScheduled] = [StatusConfirmed, StatusCompleted, StatusCancelled, StatusNoShow],
            [StatusConfirmed] = [StatusCompleted, StatusCancelled, StatusNoShow],
            [StatusCompleted] = [],
            [StatusCancelled] = [],
            [StatusNoShow] = []
        };

    /// <summary>Only these statuses occupy the doctor's schedule for overlap detection.</summary>
    private static readonly string[] BlockingStatuses = [StatusScheduled, StatusConfirmed];

    public async Task<PagedResult<AppointmentListItemDto>> GetAppointmentsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        AppointmentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

        var appointmentsQuery = dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId);

        // DOCTOR actors are always scoped to their own profile; client-supplied
        // doctorId is ignored for them (no impersonation).
        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == ownDoctorId);
        }
        else if (query.DoctorId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == query.DoctorId.Value);
        }

        if (query.Date.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate == query.Date.Value);
        }
        else
        {
            if (query.From.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate <= query.To.Value);
            }
        }

        if (query.PatientId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.PatientId == query.PatientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeStatus(query.Status);
            appointmentsQuery = appointmentsQuery.Where(a => a.Status == status);
        }

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value && !query.Date.HasValue)
        {
            throw new BusinessRuleException("Invalid date range: 'from' must not be after 'to'.");
        }

        var totalCount = await appointmentsQuery.CountAsync(cancellationToken);

        var items = await appointmentsQuery
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ThenBy(a => a.AppointmentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentListItemDto
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Reason = a.Reason
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AppointmentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AppointmentDetailDto?> GetAppointmentByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong appointmentId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.AppointmentId == appointmentId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(a => a.DoctorId == ownDoctorId);
        }

        return await query
            .Select(a => new AppointmentDetailDto
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Reason = a.Reason,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AppointmentDetailDto> CreateAppointmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.PatientId.HasValue)
        {
            throw new BusinessRuleException("Patient is required.");
        }

        var effectiveDoctorId = await ResolveEffectiveDoctorIdAsync(
            clinicId, actorUserId, actorRole, request.DoctorId, cancellationToken);

        await ValidatePatientAsync(clinicId, request.PatientId.Value, cancellationToken);
        await ValidateDoctorAsync(clinicId, effectiveDoctorId, cancellationToken);

        var date = request.AppointmentDate!.Value;
        var startTime = request.StartTime!.Value;
        var endTime = request.EndTime!.Value;

        await ValidateSchedulingRulesAsync(clinicId, date, startTime, endTime, cancellationToken);

        var now = DateTime.UtcNow;
        var appointment = new Models.Appointment
        {
            ClinicId = clinicId,
            PatientId = request.PatientId.Value,
            DoctorId = effectiveDoctorId,
            AppointmentDate = date,
            StartTime = startTime,
            EndTime = endTime,
            Status = StatusScheduled,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedBy = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Serialize concurrent bookings for the same doctor: lock the doctor row,
        // re-check overlaps inside the transaction, then insert.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await LockDoctorRowAsync(effectiveDoctorId, cancellationToken);
            await EnsureNoOverlapAsync(clinicId, effectiveDoctorId, date, startTime, endTime, excludeAppointmentId: null, cancellationToken);

            dbContext.Appointments.Add(appointment);
            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.Create,
                AuditEntities.Appointment,
                entityId: appointment.AppointmentId,
                newData: Snapshot(appointment));

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (BusinessRuleException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BusinessRuleException("Unable to create the appointment.");
        }

        return await GetAppointmentByIdAsync(clinicId, actorUserId, actorRole, appointment.AppointmentId, cancellationToken)
               ?? throw new BusinessRuleException("Appointment was created but could not be loaded.");
    }

    public async Task<AppointmentDetailDto?> UpdateAppointmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong appointmentId,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointment = await LoadScopedAppointmentAsync(
            clinicId, actorUserId, actorRole, appointmentId, tracked: true, cancellationToken);

        if (appointment is null)
        {
            return null;
        }

        if (appointment.Status != StatusScheduled && appointment.Status != StatusConfirmed)
        {
            throw new BusinessRuleException(
                $"Only SCHEDULED or CONFIRMED appointments can be modified. Current status: {appointment.Status}.");
        }

        var oldSnapshot = Snapshot(appointment);

        var effectiveDoctorId = appointment.DoctorId;
        if (actorRole == AppRoles.Doctor)
        {
            // A doctor can never reassign an appointment to another doctor.
            effectiveDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
        }
        else if (request.DoctorId.HasValue && request.DoctorId.Value != appointment.DoctorId)
        {
            effectiveDoctorId = request.DoctorId.Value;
            await ValidateDoctorAsync(clinicId, effectiveDoctorId, cancellationToken);
        }

        var newPatientId = request.PatientId ?? appointment.PatientId;
        if (newPatientId != appointment.PatientId)
        {
            await ValidatePatientAsync(clinicId, newPatientId, cancellationToken);
        }

        var newDate = request.AppointmentDate ?? appointment.AppointmentDate;
        var newStart = request.StartTime ?? appointment.StartTime;
        var newEnd = request.EndTime ?? appointment.EndTime;

        var schedulingChanged =
            newDate != appointment.AppointmentDate ||
            newStart != appointment.StartTime ||
            newEnd != appointment.EndTime ||
            effectiveDoctorId != appointment.DoctorId;

        if (schedulingChanged)
        {
            await ValidateSchedulingRulesAsync(clinicId, newDate, newStart, newEnd, cancellationToken);
        }

        appointment.PatientId = newPatientId;
        appointment.DoctorId = effectiveDoctorId;
        appointment.AppointmentDate = newDate;
        appointment.StartTime = newStart;
        appointment.EndTime = newEnd;

        if (request.Reason is not null)
        {
            appointment.Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        }

        if (request.Notes is not null)
        {
            appointment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        appointment.UpdatedAt = DateTime.UtcNow;

        if (schedulingChanged)
        {
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await LockDoctorRowAsync(effectiveDoctorId, cancellationToken);
                await EnsureNoOverlapAsync(
                    clinicId, effectiveDoctorId, newDate, newStart, newEnd,
                    excludeAppointmentId: appointment.AppointmentId, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                auditService.Record(
                    actorUserId,
                    clinicId,
                    AuditActions.Update,
                    AuditEntities.Appointment,
                    entityId: appointment.AppointmentId,
                    newData: Snapshot(appointment),
                    oldData: oldSnapshot);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (BusinessRuleException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new BusinessRuleException("Unable to update the appointment.");
            }
        }
        else
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.Update,
                AuditEntities.Appointment,
                entityId: appointment.AppointmentId,
                newData: Snapshot(appointment),
                oldData: oldSnapshot);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Re-fetch so the response includes patient/doctor names.
        return await GetAppointmentByIdAsync(
            clinicId, actorUserId, actorRole, appointmentId, cancellationToken);
    }

    public Task<AppointmentDetailDto?> ConfirmAppointmentAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(clinicId, actorUserId, actorRole, appointmentId, StatusConfirmed, AuditActions.Confirm, cancellationToken);

    public Task<AppointmentDetailDto?> CompleteAppointmentAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(clinicId, actorUserId, actorRole, appointmentId, StatusCompleted, AuditActions.Complete, cancellationToken);

    public Task<AppointmentDetailDto?> CancelAppointmentAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(clinicId, actorUserId, actorRole, appointmentId, StatusCancelled, AuditActions.Cancel, cancellationToken);

    public Task<AppointmentDetailDto?> MarkNoShowAsync(
        ulong clinicId, ulong actorUserId, string actorRole, ulong appointmentId,
        CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(clinicId, actorUserId, actorRole, appointmentId, StatusNoShow, AuditActions.NoShow, cancellationToken);

    private async Task<AppointmentDetailDto?> ChangeStatusAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong appointmentId,
        string targetStatus,
        string auditAction,
        CancellationToken cancellationToken)
    {
        var appointment = await LoadScopedAppointmentAsync(
            clinicId, actorUserId, actorRole, appointmentId, tracked: true, cancellationToken);

        if (appointment is null)
        {
            return null;
        }

        var currentStatus = appointment.Status;
        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(targetStatus))
        {
            throw new BusinessRuleException(
                $"Invalid status transition: {currentStatus} → {targetStatus}.");
        }

        appointment.Status = targetStatus;
        appointment.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            auditAction,
            AuditEntities.Appointment,
            entityId: appointment.AppointmentId,
            newData: new { From = currentStatus, To = targetStatus },
            oldData: new { Status = currentStatus });

        await dbContext.SaveChangesAsync(cancellationToken);

        // Re-fetch so the response includes patient/doctor names.
        return await GetAppointmentByIdAsync(
            clinicId, actorUserId, actorRole, appointmentId, cancellationToken);
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>
    /// Resolves the doctor profile id for a DOCTOR actor from the authenticated user.
    /// Never trusts a client-supplied doctor id for doctor actors.
    /// </summary>
    private async Task<ulong> ResolveActorDoctorIdAsync(
        ulong actorUserId,
        CancellationToken cancellationToken)
    {
        var doctorId = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.UserId == actorUserId)
            .Select(d => (ulong?)d.DoctorId)
            .FirstOrDefaultAsync(cancellationToken);

        return doctorId
            ?? throw new BusinessRuleException(
                "The authenticated user has no linked doctor profile.");
    }

    /// <summary>
    /// Determines which doctor an appointment belongs to:
    /// DOCTOR actors are forced onto their own profile; ADMIN/SECRETARY must supply one.
    /// </summary>
    private async Task<ulong> ResolveEffectiveDoctorIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong? requestedDoctorId,
        CancellationToken cancellationToken)
    {
        if (actorRole == AppRoles.Doctor)
        {
            return await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
        }

        if (!requestedDoctorId.HasValue)
        {
            throw new BusinessRuleException("Doctor is required.");
        }

        return requestedDoctorId.Value;
    }

    private async Task ValidatePatientAsync(
        ulong clinicId,
        ulong patientId,
        CancellationToken cancellationToken)
    {
        var patient = await dbContext.Patients
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .Select(p => new { p.ClinicId, p.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (patient is null || patient.ClinicId != clinicId)
        {
            throw new BusinessRuleException("Patient not found in this clinic.");
        }

        // Documented rule: inactive patients cannot receive new appointments.
        if (patient.IsActive == false)
        {
            throw new BusinessRuleException("Cannot schedule appointments for an inactive patient.");
        }
    }

    private async Task ValidateDoctorAsync(
        ulong clinicId,
        ulong doctorId,
        CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.DoctorId == doctorId)
            .Select(d => new { d.ClinicId, d.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (doctor is null || doctor.ClinicId != clinicId)
        {
            throw new BusinessRuleException("Doctor not found in this clinic.");
        }

        if (doctor.IsActive == false)
        {
            throw new BusinessRuleException("Cannot schedule appointments for an inactive doctor.");
        }
    }

    /// <summary>
    /// Validates time ordering, clinic working hours for the day, and slot-grid
    /// alignment based on the clinic_settings key 'appointment_slot_minutes'.
    /// All times are clinic-local wall-clock times (Asia/Gaza); no UTC conversion.
    /// </summary>
    private async Task ValidateSchedulingRulesAsync(
        ulong clinicId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken)
    {
        if (startTime >= endTime)
        {
            throw new BusinessRuleException(
                "Start time must be before end time; zero-duration appointments are not allowed.");
        }

        var dayOfWeek = (sbyte)(int)date.DayOfWeek; // Sunday=0 … Saturday=6, matches schema CHECK 0–6.

        var workingHours = await dbContext.ClinicWorkingHours
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.ClinicId == clinicId && w.DayOfWeek == dayOfWeek,
                cancellationToken);

        if (workingHours is null || workingHours.IsOpen != true)
        {
            throw new BusinessRuleException("The clinic is closed on the selected day.");
        }

        if (startTime < workingHours.OpeningTime!.Value || endTime > workingHours.ClosingTime!.Value)
        {
            throw new BusinessRuleException(
                $"Appointment must be within working hours " +
                $"{workingHours.OpeningTime:HH\\:mm}–{workingHours.ClosingTime:HH\\:mm}.");
        }

        var slotMinutes = await GetSlotMinutesAsync(clinicId, cancellationToken);
        if (slotMinutes > 0)
        {
            var startTotalMinutes = startTime.Hour * 60 + startTime.Minute;
            if (startTotalMinutes % slotMinutes != 0 || startTime.Second != 0)
            {
                throw new BusinessRuleException(
                    $"Appointment start time must align to {slotMinutes}-minute slots " +
                    "(e.g., 09:00, 09:30, 10:00).");
            }
        }
    }

    private async Task<int> GetSlotMinutesAsync(
        ulong clinicId,
        CancellationToken cancellationToken)
    {
        var setting = await dbContext.ClinicSettings
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.SettingKey == "appointment_slot_minutes")
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync(cancellationToken);

        return int.TryParse(setting, out var slot) && slot > 0 ? slot : 0;
    }

    /// <summary>
    /// Takes a pessimistic lock on the doctor row so two simultaneous requests for the
    /// same doctor serialize their overlap checks (practical MariaDB strategy; no
    /// distributed locking infrastructure required).
    /// Must be called inside an active transaction.
    /// </summary>
    private async Task LockDoctorRowAsync(ulong doctorId, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlAsync(
            $"SELECT doctor_id FROM doctors WHERE doctor_id = {doctorId} FOR UPDATE");
    }

    /// <summary>
    /// Two active appointments overlap when existing.start < requested.end AND
    /// existing.end > requested.start. Only SCHEDULED and CONFIRMED block time;
    /// CANCELLED, NO_SHOW and COMPLETED never conflict. Back-to-back is allowed.
    /// </summary>
    private async Task EnsureNoOverlapAsync(
        ulong clinicId,
        ulong doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        ulong? excludeAppointmentId,
        CancellationToken cancellationToken)
    {
        var hasConflict = await dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.ClinicId == clinicId &&
                a.DoctorId == doctorId &&
                a.AppointmentDate == date &&
                BlockingStatuses.Contains(a.Status) &&
                (excludeAppointmentId == null || a.AppointmentId != excludeAppointmentId.Value) &&
                a.StartTime < endTime &&
                a.EndTime > startTime,
                cancellationToken);

        if (hasConflict)
        {
            throw new BusinessRuleException(
                "This time slot conflicts with another active appointment for the selected doctor.");
        }
    }

    private async Task<Models.Appointment?> LoadScopedAppointmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong appointmentId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = tracked
            ? dbContext.Appointments.AsQueryable()
            : dbContext.Appointments.AsNoTracking().AsQueryable();

        query = query.Where(a => a.ClinicId == clinicId && a.AppointmentId == appointmentId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(a => a.DoctorId == ownDoctorId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim().ToUpperInvariant();
        return normalized switch
        {
            StatusScheduled => StatusScheduled,
            StatusConfirmed => StatusConfirmed,
            StatusCompleted => StatusCompleted,
            StatusCancelled => StatusCancelled,
            StatusNoShow => StatusNoShow,
            _ => throw new BusinessRuleException(
                $"Invalid status '{status}'. Allowed statuses: {StatusScheduled}, {StatusConfirmed}, {StatusCompleted}, {StatusCancelled}, {StatusNoShow}.")
        };
    }

    private static object Snapshot(Models.Appointment a) => new
    {
        a.PatientId,
        a.DoctorId,
        a.AppointmentDate,
        a.StartTime,
        a.EndTime,
        a.Status,
        a.Reason
    };

}
