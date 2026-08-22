using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class PatientTreatmentService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IPatientTreatmentService
{
    private const string StatusUnpaid = "UNPAID";

    public async Task<PagedResult<PatientTreatmentListItemDto>> GetPatientTreatmentsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        PatientTreatmentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var treatmentsQuery = dbContext.PatientTreatments
            .AsNoTracking()
            .Where(pt => pt.ClinicId == clinicId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            treatmentsQuery = treatmentsQuery.Where(pt => pt.DoctorId == ownDoctorId);
        }
        else if (query.DoctorId.HasValue)
        {
            treatmentsQuery = treatmentsQuery.Where(pt => pt.DoctorId == query.DoctorId.Value);
        }

        if (query.PatientId.HasValue)
        {
            treatmentsQuery = treatmentsQuery.Where(pt => pt.PatientId == query.PatientId.Value);
        }

        if (query.VisitId.HasValue)
        {
            treatmentsQuery = treatmentsQuery.Where(pt => pt.VisitId == query.VisitId.Value);
        }

        if (query.TreatmentId.HasValue)
        {
            treatmentsQuery = treatmentsQuery.Where(pt => pt.TreatmentId == query.TreatmentId.Value);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value.ToDateTime(TimeOnly.MinValue);
            treatmentsQuery = treatmentsQuery.Where(pt => pt.TreatmentDate >= from);
        }

        if (query.To.HasValue)
        {
            var to = query.To.Value.ToDateTime(TimeOnly.MaxValue);
            treatmentsQuery = treatmentsQuery.Where(pt => pt.TreatmentDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeStatus(query.Status);
            treatmentsQuery = treatmentsQuery.Where(pt => pt.Status == status);
        }

        var totalCount = await treatmentsQuery.CountAsync(cancellationToken);

        var items = await treatmentsQuery
            .OrderByDescending(pt => pt.TreatmentDate)
            .ThenByDescending(pt => pt.PatientTreatmentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pt => new PatientTreatmentListItemDto
            {
                PatientTreatmentId = pt.PatientTreatmentId,
                PatientId = pt.PatientId,
                PatientName = pt.Patient.FirstName + " " + pt.Patient.LastName,
                DoctorId = pt.DoctorId,
                DoctorName = pt.Doctor.User.FullName,
                VisitId = pt.VisitId,
                TreatmentId = pt.TreatmentId,
                TreatmentName = pt.TreatmentName,
                TreatmentDate = pt.TreatmentDate,
                Quantity = pt.Quantity,
                UnitPrice = pt.UnitPrice,
                DiscountAmount = pt.DiscountAmount,
                FinalAmount = pt.FinalAmount ?? 0m,
                Status = pt.Status
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PatientTreatmentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PatientTreatmentDetailDto?> GetPatientTreatmentByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientTreatmentId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PatientTreatments
            .AsNoTracking()
            .Where(pt => pt.ClinicId == clinicId && pt.PatientTreatmentId == patientTreatmentId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(pt => pt.DoctorId == ownDoctorId);
        }

        return await query
            .Select(pt => new PatientTreatmentDetailDto
            {
                PatientTreatmentId = pt.PatientTreatmentId,
                PatientId = pt.PatientId,
                PatientName = pt.Patient.FirstName + " " + pt.Patient.LastName,
                DoctorId = pt.DoctorId,
                DoctorName = pt.Doctor.User.FullName,
                VisitId = pt.VisitId,
                TreatmentId = pt.TreatmentId,
                TreatmentName = pt.TreatmentName,
                TreatmentDate = pt.TreatmentDate,
                Quantity = pt.Quantity,
                UnitPrice = pt.UnitPrice,
                DiscountAmount = pt.DiscountAmount,
                FinalAmount = pt.FinalAmount ?? 0m,
                Status = pt.Status,
                Notes = pt.Notes,
                CreatedAt = pt.CreatedAt,
                UpdatedAt = pt.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PatientTreatmentDetailDto> CreatePatientTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        CreatePatientTreatmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.PatientId.HasValue)
        {
            throw new BusinessRuleException("Patient is required.");
        }

        var effectiveDoctorId = await ResolveEffectiveDoctorIdAsync(actorUserId, actorRole, request.DoctorId, cancellationToken);

        await ValidatePatientAsync(clinicId, request.PatientId.Value, cancellationToken);
        await ValidateDoctorAsync(clinicId, effectiveDoctorId, cancellationToken);

        // Catalog snapshot: name and price are frozen at creation time.
        string treatmentName;
        decimal unitPrice;
        if (request.TreatmentId.HasValue)
        {
            var catalog = await dbContext.Treatments
                .AsNoTracking()
                .Where(t => t.TreatmentId == request.TreatmentId.Value)
                .Select(t => new { t.ClinicId, t.Name, t.DefaultPrice, t.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (catalog is null || catalog.ClinicId != clinicId)
            {
                throw new BusinessRuleException("Treatment not found in this clinic.");
            }

            // Documented rule: new patient treatments require an ACTIVE catalog item.
            if (catalog.IsActive == false)
            {
                throw new BusinessRuleException("Cannot record a patient treatment for an inactive catalog item.");
            }

            treatmentName = catalog.Name;
            unitPrice = request.UnitPrice ?? catalog.DefaultPrice;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.TreatmentName))
            {
                throw new BusinessRuleException(
                    "TreatmentName is required when no catalog TreatmentId is provided.");
            }

            if (!request.UnitPrice.HasValue)
            {
                throw new BusinessRuleException(
                    "UnitPrice is required when no catalog TreatmentId is provided.");
            }

            treatmentName = request.TreatmentName.Trim();
            unitPrice = request.UnitPrice.Value;
        }

        var quantity = request.Quantity ?? 1m;
        var discountAmount = request.DiscountAmount ?? 0m;

        if (quantity <= 0)
        {
            throw new BusinessRuleException("Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new BusinessRuleException("Unit price cannot be negative.");
        }

        if (discountAmount < 0 || discountAmount > quantity * unitPrice)
        {
            throw new BusinessRuleException("Discount must be between zero and the line subtotal (quantity × unit price).");
        }

        if (request.VisitId.HasValue)
        {
            await ValidateVisitLinkAsync(clinicId, request.PatientId.Value, request.VisitId.Value, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var patientTreatment = new Models.PatientTreatment
        {
            ClinicId = clinicId,
            PatientId = request.PatientId.Value,
            DoctorId = effectiveDoctorId,
            VisitId = request.VisitId,
            TreatmentId = request.TreatmentId,
            TreatmentName = treatmentName,
            TreatmentDate = request.TreatmentDate ?? now,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountAmount = discountAmount,
            Status = StatusUnpaid,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedBy = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.PatientTreatments.Add(patientTreatment);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.PatientTreatment,
            entityId: patientTreatment.PatientTreatmentId,
            newData: Snapshot(patientTreatment));

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetPatientTreatmentByIdAsync(
                   clinicId, actorUserId, actorRole, patientTreatment.PatientTreatmentId, cancellationToken)
               ?? throw new BusinessRuleException("Patient treatment was created but could not be loaded.");
    }

    public async Task<PatientTreatmentDetailDto?> UpdatePatientTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientTreatmentId,
        UpdatePatientTreatmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var patientTreatment = await LoadScopedPatientTreatmentAsync(
            clinicId, actorUserId, actorRole, patientTreatmentId, cancellationToken);

        if (patientTreatment is null)
        {
            return null;
        }

        var oldSnapshot = Snapshot(patientTreatment);

        var quantity = request.Quantity ?? patientTreatment.Quantity;
        var unitPrice = request.UnitPrice ?? patientTreatment.UnitPrice;
        var discountAmount = request.DiscountAmount ?? patientTreatment.DiscountAmount;

        if (quantity <= 0)
        {
            throw new BusinessRuleException("Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new BusinessRuleException("Unit price cannot be negative.");
        }

        if (discountAmount < 0 || discountAmount > quantity * unitPrice)
        {
            throw new BusinessRuleException("Discount must be between zero and the line subtotal (quantity × unit price).");
        }

        patientTreatment.Quantity = quantity;
        patientTreatment.UnitPrice = unitPrice;
        patientTreatment.DiscountAmount = discountAmount;

        if (request.VisitId.HasValue && request.VisitId.Value != patientTreatment.VisitId)
        {
            await ValidateVisitLinkAsync(clinicId, patientTreatment.PatientId, request.VisitId.Value, cancellationToken);
            patientTreatment.VisitId = request.VisitId.Value;
        }

        if (request.TreatmentDate.HasValue)
        {
            patientTreatment.TreatmentDate = request.TreatmentDate.Value;
        }

        if (request.Notes is not null)
        {
            patientTreatment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        patientTreatment.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.PatientTreatment,
            entityId: patientTreatment.PatientTreatmentId,
            newData: Snapshot(patientTreatment),
            oldData: oldSnapshot);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetPatientTreatmentByIdAsync(
            clinicId, actorUserId, actorRole, patientTreatmentId, cancellationToken);
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

        if (patient.IsActive == false)
        {
            throw new BusinessRuleException("Cannot record treatments for an inactive patient.");
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
            throw new BusinessRuleException("Cannot assign treatments to an inactive doctor.");
        }
    }

    /// <summary>
    /// A visit link must belong to the same clinic AND the same patient.
    /// The performing doctor may differ from the visit's doctor (documented rule).
    /// </summary>
    private async Task ValidateVisitLinkAsync(
        ulong clinicId,
        ulong patientId,
        ulong visitId,
        CancellationToken cancellationToken)
    {
        var visit = await dbContext.Visits
            .AsNoTracking()
            .Where(v => v.VisitId == visitId)
            .Select(v => new { v.ClinicId, v.PatientId })
            .FirstOrDefaultAsync(cancellationToken);

        if (visit is null || visit.ClinicId != clinicId)
        {
            throw new BusinessRuleException("Visit not found in this clinic.");
        }

        if (visit.PatientId != patientId)
        {
            throw new BusinessRuleException("The selected visit belongs to a different patient.");
        }
    }

    private async Task<Models.PatientTreatment?> LoadScopedPatientTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientTreatmentId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PatientTreatments
            .Where(pt => pt.ClinicId == clinicId && pt.PatientTreatmentId == patientTreatmentId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(pt => pt.DoctorId == ownDoctorId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim().ToUpperInvariant();
        return normalized switch
        {
            "UNPAID" => "UNPAID",
            "PARTIALLY_PAID" => "PARTIALLY_PAID",
            "PAID" => "PAID",
            "VOIDED" => "VOIDED",
            _ => throw new BusinessRuleException(
                $"Invalid status '{status}'. Allowed statuses: UNPAID, PARTIALLY_PAID, PAID, VOIDED.")
        };
    }

    private static object Snapshot(Models.PatientTreatment pt) => new
    {
        pt.PatientId,
        pt.DoctorId,
        pt.VisitId,
        pt.TreatmentId,
        pt.TreatmentName,
        pt.TreatmentDate,
        pt.Quantity,
        pt.UnitPrice,
        pt.DiscountAmount,
        pt.Status
    };
}