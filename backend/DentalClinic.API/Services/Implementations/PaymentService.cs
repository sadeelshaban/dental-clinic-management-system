using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class PaymentService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IPaymentService
{
    private const string StatusUnpaid = "UNPAID";
    private const string StatusPartiallyPaid = "PARTIALLY_PAID";
    private const string StatusPaid = "PAID";

    public async Task<PaymentDetailDto> CreatePaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.PatientTreatmentId.HasValue)
        {
            throw new BusinessRuleException("Patient treatment is required.");
        }

        var amount = request.Amount!.Value;
        if (amount <= 0)
        {
            throw new BusinessRuleException("Payment amount must be greater than zero.");
        }

        var method = NormalizeMethod(request.Method);

        if (request.PaymentMethodId.HasValue)
        {
            await ValidatePaymentMethodAsync(clinicId, request.PaymentMethodId.Value, cancellationToken);
        }

        // Load the tracked treatment (clinic-scoped). Its status/total are
        // server-derived; the client can never influence them.
        var treatment = await dbContext.PatientTreatments
            .FirstOrDefaultAsync(
                pt => pt.ClinicId == clinicId && pt.PatientTreatmentId == request.PatientTreatmentId.Value,
                cancellationToken);

        if (treatment is null)
        {
            throw new BusinessRuleException("Patient treatment not found in this clinic.");
        }

        if (treatment.Status == "VOIDED")
        {
            throw new BusinessRuleException("Cannot record payments for a voided treatment.");
        }

        var now = DateTime.UtcNow;

        // Serialize concurrent payments on the same treatment: lock the row,
        // recompute remaining inside the transaction, validate, insert.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await LockTreatmentRowAsync(treatment.PatientTreatmentId, cancellationToken);

            var total = treatment.FinalAmount ?? 0m;
            var paid = await GetValidPaidAmountAsync(clinicId, treatment.PatientTreatmentId, cancellationToken);
            var remaining = total - paid;

            if (remaining <= 0)
            {
                throw new BusinessRuleException("This treatment is already fully paid.");
            }

            if (amount > remaining)
            {
                throw new BusinessRuleException(
                    $"Overpayment rejected: remaining balance is {remaining:0.00}.");
            }

            var payment = new Models.PatientPayment
            {
                ClinicId = clinicId,
                PatientId = treatment.PatientId,
                PatientTreatmentId = treatment.PatientTreatmentId,
                Amount = amount,
                PaymentDate = request.PaymentDate ?? now,
                Method = method,
                PaymentMethodId = request.PaymentMethodId,
                ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? null : request.ReferenceNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                ReceivedBy = actorUserId,
                IsVoided = false,
                CreatedAt = now
            };

            dbContext.PatientPayments.Add(payment);

            // Server-derived status (never client-set).
            treatment.Status = DeriveStatus(paid + amount, total);
            treatment.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.PaymentCreated,
                AuditEntities.Payment,
                entityId: payment.PaymentId,
                newData: new
                {
                    payment.PatientTreatmentId,
                    payment.Amount,
                    payment.Method,
                    TreatmentStatusAfter = treatment.Status
                });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (await GetPaymentByIdInternalAsync(clinicId, payment.PaymentId, cancellationToken))!;
        }
        catch (BusinessRuleException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BusinessRuleException("Unable to record the payment.");
        }
    }

    public async Task<PaymentDetailDto?> VoidPaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong paymentId,
        VoidPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var payment = await dbContext.PatientPayments
            .FirstOrDefaultAsync(
                p => p.ClinicId == clinicId && p.PaymentId == paymentId,
                cancellationToken);

        if (payment is null)
        {
            return null;
        }

        if (payment.IsVoided)
        {
            throw new BusinessRuleException("This payment has already been voided.");
        }

        var reason = request.Reason.Trim();

        var now = DateTime.UtcNow;

        // Lock the treatment row so a concurrent payment cannot interleave with
        // the status recomputation triggered by this void.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await LockTreatmentRowAsync(payment.PatientTreatmentId, cancellationToken);

            var treatment = await dbContext.PatientTreatments
                .FirstAsync(
                    pt => pt.ClinicId == clinicId && pt.PatientTreatmentId == payment.PatientTreatmentId,
                    cancellationToken);

            var total = treatment.FinalAmount ?? 0m;
            var paidBefore = await GetValidPaidAmountAsync(clinicId, treatment.PatientTreatmentId, cancellationToken);

            payment.IsVoided = true;
            payment.VoidedAt = now;
            payment.VoidedBy = actorUserId;
            payment.VoidReason = reason;

            // Voided payments stop counting toward totals/revenue immediately.
            treatment.Status = DeriveStatus(paidBefore - payment.Amount, total);
            treatment.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.PaymentVoided,
                AuditEntities.Payment,
                entityId: payment.PaymentId,
                newData: new { payment.Amount, Reason = reason, TreatmentStatusAfter = treatment.Status },
                oldData: new { IsVoided = false });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (await GetPaymentByIdInternalAsync(clinicId, payment.PaymentId, cancellationToken))!;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BusinessRuleException("Unable to void the payment.");
        }
    }

    public async Task<PagedResult<PaymentListItemDto>> GetPaymentsAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        PaymentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var paymentsQuery = dbContext.PatientPayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            paymentsQuery = paymentsQuery.Where(p => p.PatientTreatment.DoctorId == ownDoctorId);
        }

        if (query.PatientId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.PatientId == query.PatientId.Value);
        }

        if (query.PatientTreatmentId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.PatientTreatmentId == query.PatientTreatmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Method))
        {
            var method = NormalizeMethod(query.Method);
            paymentsQuery = paymentsQuery.Where(p => p.Method == method);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value.ToDateTime(TimeOnly.MinValue);
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= from);
        }

        if (query.To.HasValue)
        {
            var to = query.To.Value.ToDateTime(TimeOnly.MaxValue);
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= to);
        }

        if (query.IsVoided.HasValue)
        {
            paymentsQuery = query.IsVoided.Value
                ? paymentsQuery.Where(p => p.IsVoided)
                : paymentsQuery.Where(p => !p.IsVoided);
        }

        var totalCount = await paymentsQuery.CountAsync(cancellationToken);

        var items = await paymentsQuery
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.PaymentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentListItemDto
            {
                PaymentId = p.PaymentId,
                PatientId = p.PatientId,
                PatientName = p.Patient.FirstName + " " + p.Patient.LastName,
                PatientTreatmentId = p.PatientTreatmentId,
                TreatmentName = p.PatientTreatment.TreatmentName,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                PaymentMethodId = p.PaymentMethodId,
                ReferenceNumber = p.ReferenceNumber,
                IsVoided = p.IsVoided
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaymentDetailDto?> GetPaymentByIdAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong paymentId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PatientPayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.PaymentId == paymentId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            query = query.Where(p => p.PatientTreatment.DoctorId == ownDoctorId);
        }

        return await SelectPaymentDetail(query).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PatientFinancialStatementDto?> GetPatientFinancialStatementAsync(
        ulong clinicId,
        ulong actorUserId,
        string actorRole,
        ulong patientId,
        CancellationToken cancellationToken = default)
    {
        var patientExists = await dbContext.Patients
            .AsNoTracking()
            .AnyAsync(p => p.ClinicId == clinicId && p.PatientId == patientId, cancellationToken);

        if (!patientExists)
        {
            return null;
        }

        // Summary from the existing patient_financial_summary view (authoritative).
        var summary = await dbContext.PatientFinancialSummaries
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.PatientId == patientId)
            .Select(s => new
            {
                s.FullName,
                s.PatientNumber,
                TotalTreatments = s.TotalTreatments ?? 0m,
                TotalPaid = s.TotalPaid ?? 0m,
                TotalRemaining = s.TotalRemaining ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Lines from the existing patient_treatment_financials view (authoritative),
        // joined for doctor names. DOCTOR actors see only their own lines.
        var linesQuery =
            from f in dbContext.PatientTreatmentFinancials.AsNoTracking()
            join d in dbContext.Doctors.AsNoTracking() on f.DoctorId equals d.DoctorId
            join u in dbContext.Users.AsNoTracking() on d.UserId equals u.UserId
            where f.ClinicId == clinicId && f.PatientId == patientId
            select new StatementLineDto
            {
                PatientTreatmentId = f.PatientTreatmentId,
                TreatmentDate = f.TreatmentDate,
                TreatmentName = f.TreatmentName,
                DoctorId = f.DoctorId,
                DoctorName = u.FullName,
                TreatmentTotal = f.TreatmentTotal ?? 0m,
                Paid = f.TotalPaid ?? 0m,
                Remaining = f.RemainingBalance ?? 0m
            };

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            linesQuery = linesQuery.Where(l => l.DoctorId == ownDoctorId);
        }

        var lines = await linesQuery
            .OrderByDescending(l => l.TreatmentDate)
            .ToListAsync(cancellationToken);

        var paymentsQuery = dbContext.PatientPayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.PatientId == patientId);

        if (actorRole == AppRoles.Doctor)
        {
            var ownDoctorId2 = await ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            paymentsQuery = paymentsQuery.Where(p => p.PatientTreatment.DoctorId == ownDoctorId2);
        }

        var payments = await paymentsQuery
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.PaymentId)
            .Select(p => new PaymentListItemDto
            {
                PaymentId = p.PaymentId,
                PatientId = p.PatientId,
                PatientName = p.Patient.FirstName + " " + p.Patient.LastName,
                PatientTreatmentId = p.PatientTreatmentId,
                TreatmentName = p.PatientTreatment.TreatmentName,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                PaymentMethodId = p.PaymentMethodId,
                ReferenceNumber = p.ReferenceNumber,
                IsVoided = p.IsVoided
            })
            .ToListAsync(cancellationToken);

        return new PatientFinancialStatementDto
        {
            PatientId = patientId,
            PatientName = summary?.FullName ?? string.Empty,
            PatientNumber = summary?.PatientNumber ?? string.Empty,
            TotalTreatments = summary?.TotalTreatments ?? 0m,
            TotalPaid = summary?.TotalPaid ?? 0m,
            TotalRemaining = summary?.TotalRemaining ?? 0m,
            Lines = lines,
            Payments = payments
        };
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>Sum of non-voided payments for a treatment (the only amounts that count).</summary>
    private async Task<decimal> GetValidPaidAmountAsync(
        ulong clinicId,
        ulong patientTreatmentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.PatientPayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId
                        && p.PatientTreatmentId == patientTreatmentId
                        && !p.IsVoided)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    /// <summary>
    /// Status derivation: UNPAID when no valid payments, PAID when valid payments
    /// cover the total, PARTIALLY_PAID in between. Overpayments never occur because
    /// creation rejects amount > remaining.
    /// </summary>
    private static string DeriveStatus(decimal validPaid, decimal total) =>
        validPaid <= 0 ? StatusUnpaid :
        validPaid >= total ? StatusPaid :
        StatusPartiallyPaid;

    /// <summary>Pessimistic lock serializing concurrent payments/voids per treatment.</summary>
    private async Task LockTreatmentRowAsync(ulong patientTreatmentId, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlAsync(
            $"SELECT patient_treatment_id FROM patient_treatments WHERE patient_treatment_id = {patientTreatmentId} FOR UPDATE");
    }

    private async Task ValidatePaymentMethodAsync(
        ulong clinicId,
        ulong paymentMethodId,
        CancellationToken cancellationToken)
    {
        var method = await dbContext.PaymentMethods
            .AsNoTracking()
            .Where(m => m.PaymentMethodId == paymentMethodId)
            .Select(m => new { m.ClinicId, m.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (method is null || method.ClinicId != clinicId)
        {
            throw new BusinessRuleException("Payment method not found in this clinic.");
        }

        if (method.IsActive == false)
        {
            throw new BusinessRuleException("The selected payment method is inactive.");
        }
    }

    private static string NormalizeMethod(string? method)
    {
        var normalized = method?.Trim().ToUpperInvariant();
        return normalized switch
        {
            "CASH" => "CASH",
            "CARD" => "CARD",
            "BANK_TRANSFER" => "BANK_TRANSFER",
            "CHEQUE" => "CHEQUE",
            "OTHER" => "OTHER",
            _ => throw new BusinessRuleException(
                $"Invalid payment method '{method}'. Allowed methods: CASH, CARD, BANK_TRANSFER, CHEQUE, OTHER.")
        };
    }

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

    private async Task<PaymentDetailDto?> GetPaymentByIdInternalAsync(
        ulong clinicId,
        ulong paymentId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PatientPayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.PaymentId == paymentId);

        return await SelectPaymentDetail(query).FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<PaymentDetailDto> SelectPaymentDetail(IQueryable<Models.PatientPayment> query) =>
        query.Select(p => new PaymentDetailDto
        {
            PaymentId = p.PaymentId,
            PatientId = p.PatientId,
            PatientName = p.Patient.FirstName + " " + p.Patient.LastName,
            PatientTreatmentId = p.PatientTreatmentId,
            TreatmentName = p.PatientTreatment.TreatmentName,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            Method = p.Method,
            PaymentMethodId = p.PaymentMethodId,
            ReferenceNumber = p.ReferenceNumber,
            IsVoided = p.IsVoided,
            Notes = p.Notes,
            VoidReason = p.VoidReason,
            VoidedAt = p.VoidedAt,
            CreatedAt = p.CreatedAt
        });
}