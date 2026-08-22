using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class SupplierService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : ISupplierService
{
    public async Task<PagedResult<SupplierDto>> GetSuppliersAsync(
        ulong clinicId,
        SupplierSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var suppliersQuery = dbContext.Suppliers
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId);

        if (query.IsActive.HasValue)
        {
            suppliersQuery = query.IsActive.Value
                ? suppliersQuery.Where(s => s.IsActive != false)
                : suppliersQuery.Where(s => s.IsActive == false);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            suppliersQuery = suppliersQuery.Where(s =>
                s.Name.Contains(term) ||
                (s.ContactPerson != null && s.ContactPerson.Contains(term)));
        }

        var items = await suppliersQuery
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                Name = s.Name,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                ContactPerson = s.ContactPerson,
                Notes = s.Notes,
                IsActive = s.IsActive != false,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<SupplierDto>
        {
            Items = items,
            Page = 1,
            PageSize = items.Count,
            TotalCount = items.Count
        };
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(
        ulong clinicId,
        ulong supplierId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Suppliers
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.SupplierId == supplierId)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                Name = s.Name,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                ContactPerson = s.ContactPerson,
                Notes = s.Notes,
                IsActive = s.IsActive != false,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupplierDto> CreateSupplierAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var duplicate = await dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(s => s.ClinicId == clinicId && s.Name.ToLower() == name.ToLower(), cancellationToken);

        if (duplicate)
        {
            throw new BusinessRuleException("A supplier with this name already exists.");
        }

        var supplier = new Models.Supplier
        {
            ClinicId = clinicId,
            Name = name,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            ContactPerson = string.IsNullOrWhiteSpace(request.ContactPerson) ? null : request.ContactPerson.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Suppliers.Add(supplier);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.Supplier,
            entityId: supplier.SupplierId,
            newData: new { supplier.Name, supplier.Phone, supplier.Email, supplier.ContactPerson });

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetSupplierByIdAsync(clinicId, supplier.SupplierId, cancellationToken))!;
    }

    public async Task<SupplierDto?> UpdateSupplierAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong supplierId,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.ClinicId == clinicId && s.SupplierId == supplierId, cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        var oldSnapshot = new { supplier.Name, supplier.Phone, supplier.Email, supplier.Address, supplier.ContactPerson, supplier.Notes, supplier.IsActive };

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            if (!string.Equals(name, supplier.Name, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await dbContext.Suppliers
                    .AsNoTracking()
                    .AnyAsync(s => s.ClinicId == clinicId
                                   && s.SupplierId != supplierId
                                   && s.Name.ToLower() == name.ToLower(), cancellationToken);

                if (duplicate)
                {
                    throw new BusinessRuleException("A supplier with this name already exists.");
                }

                supplier.Name = name;
            }
        }

        if (request.Phone is not null)
        {
            supplier.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        }

        if (request.Email is not null)
        {
            supplier.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        }

        if (request.Address is not null)
        {
            supplier.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        }

        if (request.ContactPerson is not null)
        {
            supplier.ContactPerson = string.IsNullOrWhiteSpace(request.ContactPerson) ? null : request.ContactPerson.Trim();
        }

        if (request.Notes is not null)
        {
            supplier.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        var wasActive = supplier.IsActive != false;
        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            supplier.IsActive = request.IsActive.Value;
        }

        supplier.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.Supplier,
            entityId: supplier.SupplierId,
            newData: new { supplier.Name, supplier.Phone, supplier.Email, supplier.Address, supplier.ContactPerson, supplier.Notes, supplier.IsActive },
            oldData: oldSnapshot);

        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            auditService.Record(
                actorUserId,
                clinicId,
                request.IsActive.Value ? AuditActions.Activate : AuditActions.Deactivate,
                AuditEntities.Supplier,
                entityId: supplier.SupplierId,
                newData: new { supplier.Name, IsActive = request.IsActive.Value });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetSupplierByIdAsync(clinicId, supplierId, cancellationToken))!;
    }

    public async Task<SupplierFinancialStatementDto?> GetSupplierFinancialStatementAsync(
        ulong clinicId,
        ulong supplierId,
        CancellationToken cancellationToken = default)
    {
        // Totals from the existing supplier_financial_summary view (authoritative).
        var summary = await dbContext.SupplierFinancialSummaries
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.SupplierId == supplierId)
            .Select(s => new
            {
                s.Name,
                TotalTransactions = s.TotalTransactions,
                TotalPurchases = s.TotalPurchases ?? 0m,
                TotalPaid = s.TotalPaid ?? 0m,
                TotalRemaining = s.TotalRemaining ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary is null)
        {
            return null; // Supplier does not exist in this clinic.
        }

        // Lines from the existing expense_financials view (authoritative),
        // joined for category names.
        var linesQuery =
            from f in dbContext.ExpenseFinancials.AsNoTracking()
            join c in dbContext.ExpenseCategories.AsNoTracking() on f.CategoryId equals c.CategoryId into cats
            from cat in cats.DefaultIfEmpty()
            where f.ClinicId == clinicId && f.SupplierId == supplierId
            select new SupplierStatementLineDto
            {
                ExpenseId = f.ExpenseId,
                ExpenseDate = f.ExpenseDate,
                DueDate = f.DueDate,
                ExpenseType = f.ExpenseType,
                CategoryName = cat != null ? cat.Name : null,
                Description = f.Description,
                TotalAmount = f.TotalAmount,
                Paid = f.TotalPaid ?? 0m,
                Remaining = f.RemainingBalance ?? 0m
            };

        var lines = await linesQuery
            .OrderByDescending(l => l.ExpenseDate)
            .ToListAsync(cancellationToken);

        var payments = await dbContext.ExpensePayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.Expense.SupplierId == supplierId)
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.ExpensePaymentId)
            .Select(p => new ExpensePaymentListItemDto
            {
                ExpensePaymentId = p.ExpensePaymentId,
                ExpenseId = p.ExpenseId,
                SupplierName = summary.Name,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                PaymentMethodId = p.PaymentMethodId,
                ReferenceNumber = p.ReferenceNumber,
                IsVoided = p.IsVoided
            })
            .ToListAsync(cancellationToken);

        return new SupplierFinancialStatementDto
        {
            SupplierId = supplierId,
            SupplierName = summary.Name,
            TotalTransactions = summary.TotalTransactions,
            TotalPurchases = summary.TotalPurchases,
            TotalPaid = summary.TotalPaid,
            TotalRemaining = summary.TotalRemaining,
            Lines = lines,
            Payments = payments
        };
    }
}