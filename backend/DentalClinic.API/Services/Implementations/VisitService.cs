using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class VisitService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IVisitService
{
    public async Task<PagedResult<VisitListItemDto>> GetVisitsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        VisitSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var visitsQuery = dbContext.Visits
            .AsNoTracking()
            .Where(v => v.ClinicId == clinicId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            visitsQuery = visitsQuery.Where(v => v.DoctorId == ownDoctorId);
        }
        else if (query.DoctorId.HasValue)
        {
            visitsQuery = visitsQuery.Where(v => v.DoctorId == query.DoctorId.Value);
        }

        if (query.PatientId.HasValue)
        {
            visitsQuery = visitsQuery.Where(v => v.PatientId == query.PatientId.Value);
        }

        if (query.Date.HasValue)
        {
            var dayStart = query.Date.Value.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);
            visitsQuery = visitsQuery.Where(v => v.VisitDate >= dayStart && v.VisitDate < dayEnd);
        }
        else
        {
            if (query.From.HasValue)
            {
                var from = query.From.Value.ToDateTime(TimeOnly.MinValue);
                visitsQuery = visitsQuery.Where(v => v.VisitDate >= from);
            }

            if (query.To.HasValue)
            {
                var to = query.To.Value.ToDateTime(TimeOnly.MaxValue);
                visitsQuery = visitsQuery.Where(v => v.VisitDate <= to);
            }
        }

        var totalCount = await visitsQuery.CountAsync(cancellationToken);

        var items = await visitsQuery
            .OrderByDescending(v => v.VisitDate)
            .ThenByDescending(v => v.VisitId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VisitListItemDto
            {
                VisitId = v.VisitId,
                PatientId = v.PatientId,
                PatientName = v.Patient.FirstName + " " + v.Patient.LastName,
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor.User.FullName,
                VisitDate = v.VisitDate,
                ChiefComplaint = v.ChiefComplaint,
                FollowUpDate = v.FollowUpDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<VisitListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<VisitDetailDto?> GetVisitByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong visitId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Visits
            .AsNoTracking()
            .Where(v => v.ClinicId == clinicId && v.VisitId == visitId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(v => v.DoctorId == ownDoctorId);
        }

        return await query
            .Select(v => new VisitDetailDto
            {
                VisitId = v.VisitId,
                PatientId = v.PatientId,
                PatientName = v.Patient.FirstName + " " + v.Patient.LastName,
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor.User.FullName,
                VisitDate = v.VisitDate,
                ChiefComplaint = v.ChiefComplaint,
                Diagnosis = v.Diagnosis,
                ClinicalNotes = v.ClinicalNotes,
                FollowUpDate = v.FollowUpDate,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VisitDetailDto> CreateVisitAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        CreateVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.PatientId.HasValue)
        {
            throw new BusinessRuleException("Patient is required.");
        }

        var effectiveDoctorId = await ResolveEffectiveDoctorIdAsync(actorUserId, actorRole, request.DoctorId, cancellationToken);

        await ValidatePatientAsync(clinicId, request.PatientId.Value, cancellationToken);
        await ValidateDoctorAsync(clinicId, effectiveDoctorId, cancellationToken);

        var now = DateTime.UtcNow;
        var visit = new Models.Visit
        {
            ClinicId = clinicId,
            PatientId = request.PatientId.Value,
            DoctorId = effectiveDoctorId,
            VisitDate = request.VisitDate!.Value,
            ChiefComplaint = string.IsNullOrWhiteSpace(request.ChiefComplaint) ? null : request.ChiefComplaint.Trim(),
            Diagnosis = string.IsNullOrWhiteSpace(request.Diagnosis) ? null : request.Diagnosis.Trim(),
            ClinicalNotes = string.IsNullOrWhiteSpace(request.ClinicalNotes) ? null : request.ClinicalNotes.Trim(),
            FollowUpDate = request.FollowUpDate,
            CreatedBy = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Visits.Add(visit);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.Visit,
            entityId: visit.VisitId,
            newData: Snapshot(visit));

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetVisitByIdAsync(clinicId, actorUserId, actorRole, visit.VisitId, cancellationToken)
               ?? throw new BusinessRuleException("Visit was created but could not be loaded.");
    }

    public async Task<VisitDetailDto?> UpdateVisitAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong visitId,
        UpdateVisitRequest request,
        CancellationToken cancellationToken = default)
    {
        var visit = await LoadScopedVisitAsync(clinicId, actorUserId, actorRole, visitId, cancellationToken);

        if (visit is null)
        {
            return null;
        }

        var oldSnapshot = Snapshot(visit);

        if (request.DoctorId.HasValue && request.DoctorId.Value != visit.DoctorId)
        {
            if (actorRole == AppRoles.Doctor)
            {
                // A doctor can never reassign a visit to another doctor.
                throw new BusinessRuleException("Doctors cannot reassign visits to another doctor.");
            }

            await ValidateDoctorAsync(clinicId, request.DoctorId.Value, cancellationToken);
            visit.DoctorId = request.DoctorId.Value;
        }

        if (request.VisitDate.HasValue)
        {
            visit.VisitDate = request.VisitDate.Value;
        }

        if (request.ChiefComplaint is not null)
        {
            visit.ChiefComplaint = string.IsNullOrWhiteSpace(request.ChiefComplaint) ? null : request.ChiefComplaint.Trim();
        }

        if (request.Diagnosis is not null)
        {
            visit.Diagnosis = string.IsNullOrWhiteSpace(request.Diagnosis) ? null : request.Diagnosis.Trim();
        }

        if (request.ClinicalNotes is not null)
        {
            visit.ClinicalNotes = string.IsNullOrWhiteSpace(request.ClinicalNotes) ? null : request.ClinicalNotes.Trim();
        }

        if (request.FollowUpDate.HasValue)
        {
            visit.FollowUpDate = request.FollowUpDate.Value;
        }

        visit.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.Visit,
            entityId: visit.VisitId,
            newData: Snapshot(visit),
            oldData: oldSnapshot);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetVisitByIdAsync(clinicId, actorUserId, actorRole, visitId, cancellationToken);
    }

    // ---------------------------------------------------------------- helpers

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
            ?? throw new BusinessRuleException("The authenticated user has no linked doctor profile.");
    }

    private async Task<ulong> ResolveEffectiveDoctorIdAsync(
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

        // Documented rule: clinical records are only created for active patients.
        if (patient.IsActive == false)
        {
            throw new BusinessRuleException("Cannot record clinical data for an inactive patient.");
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
            throw new BusinessRuleException("Cannot assign clinical records to an inactive doctor.");
        }
    }

    private async Task<Models.Visit?> LoadScopedVisitAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong visitId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Visits
            .Where(v => v.ClinicId == clinicId && v.VisitId == visitId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(v => v.DoctorId == ownDoctorId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private static object Snapshot(Models.Visit v) => new
    {
        v.PatientId,
        v.DoctorId,
        v.VisitDate,
        v.ChiefComplaint,
        v.Diagnosis,
        v.FollowUpDate
    };
}