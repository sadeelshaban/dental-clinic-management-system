using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class ExpenseService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IExpenseService
{
    private const string StatusUnpaid = "UNPAID";
    private const string StatusPartiallyPaid = "PARTIALLY_PAID";
    private const string StatusPaid = "PAID";

    // Schema ENUM values for expenses.expense_type.
    private static readonly string[] ValidExpenseTypes =
    [
        "GENERAL", "SUPPLIER_PURCHASE", "RENT", "UTILITIES", "EQUIPMENT",
        "MAINTENANCE", "LABORATORY", "MATERIALS", "OTHER"
    ];

    // ------------------------------------------------------------ expenses

    public async Task<PagedResult<ExpenseListItemDto>> GetExpensesAsync(
        ulong clinicId,
        ExpenseSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var expensesQuery = dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.ClinicId == clinicId);

        if (query.SupplierId.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.SupplierId == query.SupplierId.Value);
        }

        if (query.CategoryId.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ExpenseType))
        {
            var type = NormalizeExpenseType(query.ExpenseType);
            expensesQuery = expensesQuery.Where(e => e.ExpenseType == type);
        }

        if (query.From.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= query.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = NormalizeStatus(query.Status);
            expensesQuery = expensesQuery.Where(e => e.Status == status);
        }

        var totalCount = await expensesQuery.CountAsync(cancellationToken);

        var items = await expensesQuery
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.ExpenseId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseListItemDto
            {
                ExpenseId = e.ExpenseId,
                SupplierId = e.SupplierId,
                SupplierName = e.Supplier != null ? e.Supplier.Name : null,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                ExpenseType = e.ExpenseType,
                Description = e.Description,
                ExpenseDate = e.ExpenseDate,
                DueDate = e.DueDate,
                TotalAmount = e.TotalAmount,
                Status = e.Status
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExpenseListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ExpenseDetailDto?> GetExpenseByIdAsync(
        ulong clinicId,
        ulong expenseId,
        CancellationToken cancellationToken = default)
    {
        return await SelectExpenseDetail(dbContext.Expenses.AsNoTracking()
            .Where(e => e.ClinicId == clinicId && e.ExpenseId == expenseId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExpenseDetailDto> CreateExpenseAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TotalAmount!.Value <= 0)
        {
            throw new BusinessRuleException("Expense total must be greater than zero.");
        }

        var expenseType = NormalizeExpenseType(request.ExpenseType);

        if (request.SupplierId.HasValue)
        {
            await ValidateSupplierAsync(clinicId, request.SupplierId.Value, cancellationToken);
        }

        if (request.CategoryId.HasValue)
        {
            await ValidateCategoryAsync(clinicId, request.CategoryId.Value, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var expense = new Models.Expense
        {
            ClinicId = clinicId,
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            ExpenseType = expenseType,
            Description = request.Description.Trim(),
            ExpenseDate = request.ExpenseDate ?? DateOnly.FromDateTime(now),
            DueDate = request.DueDate,
            TotalAmount = request.TotalAmount.Value,
            Status = StatusUnpaid, // An obligation is NOT paid at creation time.
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedBy = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.Expense,
            entityId: expense.ExpenseId,
            newData: Snapshot(expense));

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetExpenseByIdAsync(clinicId, expense.ExpenseId, cancellationToken))!;
    }

    public async Task<ExpenseDetailDto?> UpdateExpenseAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong expenseId,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var expense = await dbContext.Expenses
            .FirstOrDefaultAsync(e => e.ClinicId == clinicId && e.ExpenseId == expenseId, cancellationToken);

        if (expense is null)
        {
            return null;
        }

        if (expense.Status == "VOIDED")
        {
            throw new BusinessRuleException("Cannot update a voided expense.");
        }

        var oldSnapshot = Snapshot(expense);

        if (request.SupplierId.HasValue && request.SupplierId.Value != expense.SupplierId)
        {
            await ValidateSupplierAsync(clinicId, request.SupplierId.Value, cancellationToken);
            expense.SupplierId = request.SupplierId.Value;
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value != expense.CategoryId)
        {
            await ValidateCategoryAsync(clinicId, request.CategoryId.Value, cancellationToken);
            expense.CategoryId = request.CategoryId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.ExpenseType))
        {
            expense.ExpenseType = NormalizeExpenseType(request.ExpenseType);
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            expense.Description = request.Description.Trim();
        }

        if (request.ExpenseDate.HasValue)
        {
            expense.ExpenseDate = request.ExpenseDate.Value;
        }

        if (request.DueDate.HasValue)
        {
            expense.DueDate = request.DueDate.Value;
        }

        if (request.TotalAmount.HasValue && request.TotalAmount.Value != expense.TotalAmount)
        {
            var newTotal = request.TotalAmount.Value;
            if (newTotal <= 0)
            {
                throw new BusinessRuleException("Expense total must be greater than zero.");
            }

            // Payments already recorded must still fit within the new total.
            var validPaid = await GetValidPaidAmountAsync(clinicId, expense.ExpenseId, cancellationToken);
            if (validPaid > newTotal)
            {
                throw new BusinessRuleException(
                    $"Cannot reduce the total below the already-paid amount ({validPaid:0.00}).");
            }

            expense.TotalAmount = newTotal;
            expense.Status = DeriveStatus(validPaid, newTotal);
        }

        if (request.Notes is not null)
        {
            expense.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        expense.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.Expense,
            entityId: expense.ExpenseId,
            newData: Snapshot(expense),
            oldData: oldSnapshot);

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetExpenseByIdAsync(clinicId, expenseId, cancellationToken))!;
    }

    public async Task<ExpenseDetailDto?> VoidExpenseAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong expenseId,
        VoidExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var expense = await dbContext.Expenses
            .FirstOrDefaultAsync(e => e.ClinicId == clinicId && e.ExpenseId == expenseId, cancellationToken);

        if (expense is null)
        {
            return null;
        }

        if (expense.Status == "VOIDED")
        {
            throw new BusinessRuleException("This expense has already been voided.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new BusinessRuleException("A void reason is required.");
        }

        var reason = request.Reason.Trim();
        var validPaid = await GetValidPaidAmountAsync(clinicId, expense.ExpenseId, cancellationToken);
        if (validPaid > 0)
        {
            throw new BusinessRuleException(
                "Cannot void an expense that has recorded payments. Void the payments first.");
        }

        var oldSnapshot = Snapshot(expense);
        var now = DateTime.UtcNow;

        expense.Status = "VOIDED";
        expense.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.ExpenseVoided,
            AuditEntities.Expense,
            entityId: expense.ExpenseId,
            newData: new { expense.Status, Reason = reason },
            oldData: oldSnapshot);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetExpenseByIdAsync(clinicId, expenseId, cancellationToken);
    }

    // ------------------------------------------------------------ payments

    public async Task<ExpensePaymentDetailDto> CreateExpensePaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateExpensePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.ExpenseId.HasValue)
        {
            throw new BusinessRuleException("Expense is required.");
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

        var expense = await dbContext.Expenses
            .FirstOrDefaultAsync(
                e => e.ClinicId == clinicId && e.ExpenseId == request.ExpenseId.Value,
                cancellationToken);

        if (expense is null)
        {
            throw new BusinessRuleException("Expense not found in this clinic.");
        }

        if (expense.Status == "VOIDED")
        {
            throw new BusinessRuleException("Cannot record payments for a voided expense.");
        }

        var now = DateTime.UtcNow;

        // Serialize concurrent payments on the same expense: lock the row,
        // recompute remaining inside the transaction, validate, insert.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await LockExpenseRowAsync(expense.ExpenseId, cancellationToken);

            expense = await dbContext.Expenses
                .FirstAsync(
                    e => e.ClinicId == clinicId && e.ExpenseId == expense.ExpenseId,
                    cancellationToken);

            if (expense.Status == "VOIDED")
            {
                throw new BusinessRuleException("Cannot record payments for a voided expense.");
            }

            var total = expense.TotalAmount;
            var paid = await GetValidPaidAmountAsync(clinicId, expense.ExpenseId, cancellationToken);
            var remaining = total - paid;

            if (remaining <= 0)
            {
                throw new BusinessRuleException("This expense is already fully paid.");
            }

            if (amount > remaining)
            {
                throw new BusinessRuleException(
                    $"Overpayment rejected: remaining balance is {remaining:0.00}.");
            }

            var payment = new Models.ExpensePayment
            {
                ClinicId = clinicId,
                ExpenseId = expense.ExpenseId,
                Amount = amount,
                PaymentDate = request.PaymentDate ?? now,
                Method = method,
                PaymentMethodId = request.PaymentMethodId,
                ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? null : request.ReferenceNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                PaidBy = actorUserId,
                IsVoided = false,
                CreatedAt = now
            };

            dbContext.ExpensePayments.Add(payment);

            // Server-derived status (never client-set).
            expense.Status = DeriveStatus(paid + amount, total);
            expense.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.ExpensePaymentCreated,
                AuditEntities.ExpensePayment,
                entityId: payment.ExpensePaymentId,
                newData: new
                {
                    payment.ExpenseId,
                    payment.Amount,
                    payment.Method,
                    ExpenseStatusAfter = expense.Status
                });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (await GetExpensePaymentByIdInternalAsync(clinicId, payment.ExpensePaymentId, cancellationToken))!;
        }
        catch (BusinessRuleException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BusinessRuleException("Unable to record the expense payment.");
        }
    }

    public async Task<ExpensePaymentDetailDto?> VoidExpensePaymentAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong expensePaymentId,
        VoidExpensePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var payment = await dbContext.ExpensePayments
            .FirstOrDefaultAsync(
                p => p.ClinicId == clinicId && p.ExpensePaymentId == expensePaymentId,
                cancellationToken);

        if (payment is null)
        {
            return null;
        }

        if (payment.IsVoided)
        {
            throw new BusinessRuleException("This expense payment has already been voided.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new BusinessRuleException("A void reason is required.");
        }

        var reason = request.Reason.Trim();
        var now = DateTime.UtcNow;

        // Lock the expense row so a concurrent payment cannot interleave with
        // the status recomputation triggered by this void.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await LockExpenseRowAsync(payment.ExpenseId, cancellationToken);

            var expense = await dbContext.Expenses
                .FirstAsync(
                    e => e.ClinicId == clinicId && e.ExpenseId == payment.ExpenseId,
                    cancellationToken);

            var total = expense.TotalAmount;
            var paidBefore = await GetValidPaidAmountAsync(clinicId, expense.ExpenseId, cancellationToken);

            payment.IsVoided = true;
            payment.VoidedAt = now;
            payment.VoidedBy = actorUserId;
            payment.VoidReason = reason;

            // Voided payments stop counting toward totals immediately.
            expense.Status = DeriveStatus(paidBefore - payment.Amount, total);
            expense.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.ExpensePaymentVoided,
                AuditEntities.ExpensePayment,
                entityId: payment.ExpensePaymentId,
                newData: new { payment.Amount, Reason = reason, ExpenseStatusAfter = expense.Status },
                oldData: new { IsVoided = false });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (await GetExpensePaymentByIdInternalAsync(clinicId, payment.ExpensePaymentId, cancellationToken))!;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BusinessRuleException("Unable to void the expense payment.");
        }
    }

    public async Task<PagedResult<ExpensePaymentListItemDto>> GetExpensePaymentsAsync(
        ulong clinicId,
        ExpensePaymentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var paymentsQuery = dbContext.ExpensePayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId);

        if (query.ExpenseId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.ExpenseId == query.ExpenseId.Value);
        }

        if (query.SupplierId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.Expense.SupplierId == query.SupplierId.Value);
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
            .ThenByDescending(p => p.ExpensePaymentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ExpensePaymentListItemDto
            {
                ExpensePaymentId = p.ExpensePaymentId,
                ExpenseId = p.ExpenseId,
                SupplierName = p.Expense.Supplier != null ? p.Expense.Supplier.Name : null,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                PaymentMethodId = p.PaymentMethodId,
                ReferenceNumber = p.ReferenceNumber,
                IsVoided = p.IsVoided
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExpensePaymentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ExpensePaymentDetailDto?> GetExpensePaymentByIdAsync(
        ulong clinicId,
        ulong expensePaymentId,
        CancellationToken cancellationToken = default)
    {
        return await SelectExpensePaymentDetail(dbContext.ExpensePayments.AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.ExpensePaymentId == expensePaymentId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>Sum of non-voided payments for an expense (the only amounts that count).</summary>
    private async Task<decimal> GetValidPaidAmountAsync(
        ulong clinicId,
        ulong expenseId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ExpensePayments
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId
                        && p.ExpenseId == expenseId
                        && !p.IsVoided)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    /// <summary>
    /// Status derivation: UNPAID when no valid payments, PAID when valid payments
    /// cover the total, PARTIALLY_PAID in between. Overpayments never occur because
    /// creation rejects amount > remaining and total reductions are guarded.
    /// </summary>
    private static string DeriveStatus(decimal validPaid, decimal total) =>
        validPaid <= 0 ? StatusUnpaid :
        validPaid >= total ? StatusPaid :
        StatusPartiallyPaid;

    /// <summary>Pessimistic lock serializing concurrent payments/voids per expense.</summary>
    private async Task LockExpenseRowAsync(ulong expenseId, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlAsync(
            $"SELECT expense_id FROM expenses WHERE expense_id = {expenseId} FOR UPDATE");
    }

    private async Task ValidateSupplierAsync(
        ulong clinicId,
        ulong supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierId == supplierId)
            .Select(s => new { s.ClinicId, s.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier is null || supplier.ClinicId != clinicId)
        {
            throw new BusinessRuleException("Supplier not found in this clinic.");
        }

        if (supplier.IsActive == false)
        {
            throw new BusinessRuleException("The selected supplier is inactive.");
        }
    }

    private async Task ValidateCategoryAsync(
        ulong clinicId,
        ulong categoryId,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.CategoryId == categoryId)
            .Select(c => new { c.ClinicId, c.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null || category.ClinicId != clinicId)
        {
            throw new BusinessRuleException("Expense category not found in this clinic.");
        }

        if (category.IsActive == false)
        {
            throw new BusinessRuleException("The selected expense category is inactive.");
        }
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

    private static string NormalizeExpenseType(string? type)
    {
        var normalized = type?.Trim().ToUpperInvariant();
        return normalized is not null && ValidExpenseTypes.Contains(normalized)
            ? normalized
            : throw new BusinessRuleException(
                $"Invalid expense type '{type}'. Allowed types: {string.Join(", ", ValidExpenseTypes)}.");
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

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim().ToUpperInvariant();
        return normalized switch
        {
            StatusUnpaid => StatusUnpaid,
            StatusPartiallyPaid => StatusPartiallyPaid,
            StatusPaid => StatusPaid,
            "VOIDED" => "VOIDED",
            _ => throw new BusinessRuleException(
                $"Invalid status '{status}'. Allowed statuses: {StatusUnpaid}, {StatusPartiallyPaid}, {StatusPaid}, VOIDED.")
        };
    }

    private static object Snapshot(Models.Expense e) => new
    {
        e.SupplierId,
        e.CategoryId,
        e.ExpenseType,
        e.Description,
        e.ExpenseDate,
        e.DueDate,
        e.TotalAmount,
        e.Status
    };

    private async Task<ExpensePaymentDetailDto?> GetExpensePaymentByIdInternalAsync(
        ulong clinicId,
        ulong expensePaymentId,
        CancellationToken cancellationToken)
    {
        return await SelectExpensePaymentDetail(dbContext.ExpensePayments.AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.ExpensePaymentId == expensePaymentId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ExpenseDetailDto> SelectExpenseDetail(IQueryable<Models.Expense> query) =>
        query.Select(e => new ExpenseDetailDto
        {
            ExpenseId = e.ExpenseId,
            SupplierId = e.SupplierId,
            SupplierName = e.Supplier != null ? e.Supplier.Name : null,
            CategoryId = e.CategoryId,
            CategoryName = e.Category != null ? e.Category.Name : null,
            ExpenseType = e.ExpenseType,
            Description = e.Description,
            ExpenseDate = e.ExpenseDate,
            DueDate = e.DueDate,
            TotalAmount = e.TotalAmount,
            Status = e.Status,
            Notes = e.Notes,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        });

    private static IQueryable<ExpensePaymentDetailDto> SelectExpensePaymentDetail(IQueryable<Models.ExpensePayment> query) =>
        query.Select(p => new ExpensePaymentDetailDto
        {
            ExpensePaymentId = p.ExpensePaymentId,
            ExpenseId = p.ExpenseId,
            SupplierName = p.Expense.Supplier != null ? p.Expense.Supplier.Name : null,
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