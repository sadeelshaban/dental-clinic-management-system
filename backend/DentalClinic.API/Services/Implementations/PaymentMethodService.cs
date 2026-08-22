using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class PaymentMethodService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IPaymentMethodService
{
    public async Task<PagedResult<PaymentMethodDto>> GetPaymentMethodsAsync(
        ulong clinicId,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var methodsQuery = dbContext.PaymentMethods
            .AsNoTracking()
            .Where(m => m.ClinicId == clinicId);

        if (isActive.HasValue)
        {
            methodsQuery = isActive.Value
                ? methodsQuery.Where(m => m.IsActive != false)
                : methodsQuery.Where(m => m.IsActive == false);
        }

        var items = await methodsQuery
            .OrderBy(m => m.Name)
            .Select(m => new PaymentMethodDto
            {
                PaymentMethodId = m.PaymentMethodId,
                Name = m.Name,
                IsActive = m.IsActive != false
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentMethodDto>
        {
            Items = items,
            Page = 1,
            PageSize = items.Count,
            TotalCount = items.Count
        };
    }

    public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(
        ulong clinicId,
        ulong paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PaymentMethods
            .AsNoTracking()
            .Where(m => m.ClinicId == clinicId && m.PaymentMethodId == paymentMethodId)
            .Select(m => new PaymentMethodDto
            {
                PaymentMethodId = m.PaymentMethodId,
                Name = m.Name,
                IsActive = m.IsActive != false
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(
        ulong clinicId,
        ulong actorUserId,
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var duplicate = await dbContext.PaymentMethods
            .AsNoTracking()
            .AnyAsync(m => m.ClinicId == clinicId && m.Name.ToLower() == name.ToLower(), cancellationToken);

        if (duplicate)
        {
            throw new BusinessRuleException("A payment method with this name already exists.");
        }

        var method = new Models.PaymentMethod
        {
            ClinicId = clinicId,
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.PaymentMethods.Add(method);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.PaymentMethod,
            entityId: method.PaymentMethodId,
            newData: new { method.Name });

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetPaymentMethodByIdAsync(clinicId, method.PaymentMethodId, cancellationToken))!;
    }

    public async Task<PaymentMethodDto?> UpdatePaymentMethodAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong paymentMethodId,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var method = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.PaymentMethodId == paymentMethodId, cancellationToken);

        if (method is null)
        {
            return null;
        }

        var oldSnapshot = new { method.Name, method.IsActive };

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            if (!string.Equals(name, method.Name, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await dbContext.PaymentMethods
                    .AsNoTracking()
                    .AnyAsync(m => m.ClinicId == clinicId
                                   && m.PaymentMethodId != paymentMethodId
                                   && m.Name.ToLower() == name.ToLower(), cancellationToken);

                if (duplicate)
                {
                    throw new BusinessRuleException("A payment method with this name already exists.");
                }

                method.Name = name;
            }
        }

        var wasActive = method.IsActive != false;
        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            method.IsActive = request.IsActive.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.PaymentMethod,
            entityId: method.PaymentMethodId,
            newData: new { method.Name, method.IsActive },
            oldData: oldSnapshot);

        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            auditService.Record(
                actorUserId,
                clinicId,
                request.IsActive.Value ? AuditActions.Activate : AuditActions.Deactivate,
                AuditEntities.PaymentMethod,
                entityId: method.PaymentMethodId,
                newData: new { method.Name, IsActive = request.IsActive.Value });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetPaymentMethodByIdAsync(clinicId, paymentMethodId, cancellationToken))!;
    }
}