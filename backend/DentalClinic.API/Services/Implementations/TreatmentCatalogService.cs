using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class TreatmentCatalogService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : ITreatmentCatalogService
{
    // ------------------------------------------------------------ categories

    public async Task<PagedResult<TreatmentCategoryDto>> GetCategoriesAsync(
        ulong clinicId,
        TreatmentCategorySearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

        var categoriesQuery = dbContext.TreatmentCategories
            .AsNoTracking()
            .Where(c => c.ClinicId == clinicId);

        if (query.IsActive.HasValue)
        {
            categoriesQuery = query.IsActive.Value
                ? categoriesQuery.Where(c => c.IsActive != false)
                : categoriesQuery.Where(c => c.IsActive == false);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            categoriesQuery = categoriesQuery.Where(c => c.Name.Contains(term));
        }

        var totalCount = await categoriesQuery.CountAsync(cancellationToken);

        var items = await categoriesQuery
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new TreatmentCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive != false,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TreatmentCategoryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TreatmentCategoryDto?> GetCategoryByIdAsync(
        ulong clinicId,
        ulong categoryId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TreatmentCategories
            .AsNoTracking()
            .Where(c => c.ClinicId == clinicId && c.CategoryId == categoryId)
            .Select(c => new TreatmentCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive != false,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TreatmentCategoryDto> CreateCategoryAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateTreatmentCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var duplicate = await dbContext.TreatmentCategories
            .AsNoTracking()
            .AnyAsync(c => c.ClinicId == clinicId && c.Name.ToLower() == name.ToLower(), cancellationToken);

        if (duplicate)
        {
            throw new BusinessRuleException("A treatment category with this name already exists.");
        }

        var now = DateTime.UtcNow;
        var category = new Models.TreatmentCategory
        {
            ClinicId = clinicId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.TreatmentCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.TreatmentCategory,
            entityId: category.CategoryId,
            newData: new { category.Name, category.Description });

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(clinicId, category.CategoryId, cancellationToken))!;
    }

    public async Task<TreatmentCategoryDto?> UpdateCategoryAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong categoryId,
        UpdateTreatmentCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await dbContext.TreatmentCategories
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
                var duplicate = await dbContext.TreatmentCategories
                    .AsNoTracking()
                    .AnyAsync(c => c.ClinicId == clinicId
                                   && c.CategoryId != categoryId
                                   && c.Name.ToLower() == name.ToLower(), cancellationToken);

                if (duplicate)
                {
                    throw new BusinessRuleException("A treatment category with this name already exists.");
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

        category.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.TreatmentCategory,
            entityId: category.CategoryId,
            newData: new { category.Name, category.Description, category.IsActive },
            oldData: oldSnapshot);

        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            auditService.Record(
                actorUserId,
                clinicId,
                request.IsActive.Value ? AuditActions.Activate : AuditActions.Deactivate,
                AuditEntities.TreatmentCategory,
                entityId: category.CategoryId,
                newData: new { category.Name, IsActive = request.IsActive.Value });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(clinicId, categoryId, cancellationToken))!;
    }

    // ------------------------------------------------------------ treatments

    public async Task<PagedResult<TreatmentListItemDto>> GetTreatmentsAsync(
        ulong clinicId,
        TreatmentSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

        var treatmentsQuery = dbContext.Treatments
            .AsNoTracking()
            .Where(t => t.ClinicId == clinicId);

        if (query.IsActive.HasValue)
        {
            treatmentsQuery = query.IsActive.Value
                ? treatmentsQuery.Where(t => t.IsActive != false)
                : treatmentsQuery.Where(t => t.IsActive == false);
        }

        if (query.CategoryId.HasValue)
        {
            treatmentsQuery = treatmentsQuery.Where(t => t.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            treatmentsQuery = treatmentsQuery.Where(t => t.Name.Contains(term));
        }

        var totalCount = await treatmentsQuery.CountAsync(cancellationToken);

        var items = await treatmentsQuery
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TreatmentListItemDto
            {
                TreatmentId = t.TreatmentId,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null,
                Name = t.Name,
                DefaultPrice = t.DefaultPrice,
                DurationMinutes = t.DurationMinutes,
                IsActive = t.IsActive != false
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TreatmentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TreatmentDetailDto?> GetTreatmentByIdAsync(
        ulong clinicId,
        ulong treatmentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Treatments
            .AsNoTracking()
            .Where(t => t.ClinicId == clinicId && t.TreatmentId == treatmentId)
            .Select(t => new TreatmentDetailDto
            {
                TreatmentId = t.TreatmentId,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null,
                Name = t.Name,
                Description = t.Description,
                DefaultPrice = t.DefaultPrice,
                DurationMinutes = t.DurationMinutes,
                IsActive = t.IsActive != false,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TreatmentDetailDto> CreateTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateTreatmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CategoryId.HasValue)
        {
            await ValidateCategoryAsync(clinicId, request.CategoryId.Value, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var treatment = new Models.Treatment
        {
            ClinicId = clinicId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DefaultPrice = request.DefaultPrice!.Value,
            DurationMinutes = request.DurationMinutes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Treatments.Add(treatment);
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Create,
            AuditEntities.Treatment,
            entityId: treatment.TreatmentId,
            newData: new { treatment.Name, treatment.CategoryId, treatment.DefaultPrice, treatment.DurationMinutes });

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetTreatmentByIdAsync(clinicId, treatment.TreatmentId, cancellationToken))!;
    }

    public async Task<TreatmentDetailDto?> UpdateTreatmentAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong treatmentId,
        UpdateTreatmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var treatment = await dbContext.Treatments
            .FirstOrDefaultAsync(t => t.ClinicId == clinicId && t.TreatmentId == treatmentId, cancellationToken);

        if (treatment is null)
        {
            return null;
        }

        var oldSnapshot = new { treatment.Name, treatment.CategoryId, treatment.Description, treatment.DefaultPrice, treatment.DurationMinutes, treatment.IsActive };

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            treatment.Name = request.Name.Trim();
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value != treatment.CategoryId)
        {
            await ValidateCategoryAsync(clinicId, request.CategoryId.Value, cancellationToken);
            treatment.CategoryId = request.CategoryId.Value;
        }

        if (request.Description is not null)
        {
            treatment.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        if (request.DefaultPrice.HasValue)
        {
            // Affects ONLY future patient treatments; historical snapshots are immutable.
            treatment.DefaultPrice = request.DefaultPrice.Value;
        }

        if (request.DurationMinutes.HasValue)
        {
            treatment.DurationMinutes = request.DurationMinutes.Value;
        }

        var wasActive = treatment.IsActive != false;
        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            treatment.IsActive = request.IsActive.Value;
        }

        treatment.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.Treatment,
            entityId: treatment.TreatmentId,
            newData: new { treatment.Name, treatment.CategoryId, treatment.Description, treatment.DefaultPrice, treatment.DurationMinutes, treatment.IsActive },
            oldData: oldSnapshot);

        if (request.IsActive.HasValue && request.IsActive.Value != wasActive)
        {
            auditService.Record(
                actorUserId,
                clinicId,
                request.IsActive.Value ? AuditActions.Activate : AuditActions.Deactivate,
                AuditEntities.Treatment,
                entityId: treatment.TreatmentId,
                newData: new { treatment.Name, IsActive = request.IsActive.Value });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetTreatmentByIdAsync(clinicId, treatmentId, cancellationToken))!;
    }

    // ---------------------------------------------------------------- helpers

    private async Task ValidateCategoryAsync(
        ulong clinicId,
        ulong categoryId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.TreatmentCategories
            .AsNoTracking()
            .AnyAsync(c => c.ClinicId == clinicId && c.CategoryId == categoryId, cancellationToken);

        if (!exists)
        {
            throw new BusinessRuleException("Treatment category not found in this clinic.");
        }
    }
}