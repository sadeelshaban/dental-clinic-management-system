using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class ExpenseCategoryService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IExpenseCategoryService
{
    public async Task<PagedResult<ExpenseCategoryDto>> GetCategoriesAsync(
        ulong clinicId,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var categoriesQuery = dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.ClinicId == clinicId);

        if (isActive.HasValue)
        {
            categoriesQuery = isActive.Value
                ? categoriesQuery.Where(c => c.IsActive != false)
                : categoriesQuery.Where(c => c.IsActive == false);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            categoriesQuery = categoriesQuery.Where(c => c.Name.Contains(term));
        }

        var items = await categoriesQuery
            .OrderBy(c => c.Name)
            .Select(c => new ExpenseCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive != false,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExpenseCategoryDto>
        {
            Items = items,
            Page = 1,
            PageSize = items.Count,
            TotalCount = items.Count
        };
    }

    public async Task<ExpenseCategoryDto?> GetCategoryByIdAsync(
        ulong clinicId,
        ulong categoryId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.ClinicId == clinicId && c.CategoryId == categoryId)
            .Select(c => new ExpenseCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive != false,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExpenseCategoryDto> CreateCategoryAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateExpenseCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var duplicate = await dbContext.ExpenseCategories
            .AsNoTracking()
            .AnyAsync(c => c.ClinicId == clinicId && c.Name.ToLower() == name.ToLower(), cancellationToken);

        if (duplicate)
        {
            throw new BusinessRuleException("An expense category with this name already exists.");
        }

        var category = new Models.ExpenseCategory
        {
            ClinicId = clinicId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ExpenseCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.ExpenseCategory,
            entityId: category.CategoryId,
            newData: new { category.Name, category.Description });

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(clinicId, category.CategoryId, cancellationToken))!;
    }

    public async Task<ExpenseCategoryDto?> UpdateCategoryAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong categoryId,
        UpdateExpenseCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.ExpenseCategories
            .FirstOrDefaultAsync(c => c.ClinicId == clinicId && c.CategoryId == categoryId, cancellationToken);

        if (category is null)
        {
            return null;
        }

        var oldSnapshot = new { category.Name, category.Description, category.IsActive };

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            if (!string.Equals(name, category.Name, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await dbContext.ExpenseCategories
                    .AsNoTracking()
                    .AnyAsync(c => c.ClinicId == clinicId
                                   && c.CategoryId != categoryId
                                   && c.Name.ToLower() == name.ToLower(), cancellationToken);

                if (duplicate)
                {
                    throw new BusinessRuleException("An expense category with this name already exists.");
                }

                category.Name = name;
            }
        }

        if (request.Description is not null)
        {
            category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        var wasActive = category.IsActive != false;
        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            category.IsActive = request.IsActive.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.ExpenseCategory,
            entityId: category.CategoryId,
            newData: new { category.Name, category.Description, category.IsActive },
            oldData: oldSnapshot);

        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            auditService.Record(
                actorUserId,
                clinicId,
                request.IsActive.Value ? AuditActions.Activate : AuditActions.Deactivate,
                AuditEntities.ExpenseCategory,
                entityId: category.CategoryId,
                newData: new { category.Name, IsActive = request.IsActive.Value });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(clinicId, categoryId, cancellationToken))!;
    }
}