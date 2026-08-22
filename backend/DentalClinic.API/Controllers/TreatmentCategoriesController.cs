using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Treatment category management. Reads: all clinical staff. Writes: ADMIN only
/// (per the confirmed permission matrix). Categories are soft-deactivated via PUT
/// (isActive=false) — never deleted — because historical patient treatments and
/// catalog items reference them (FK ON DELETE SET NULL would silently detach history).
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class TreatmentCategoriesController(ITreatmentCatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TreatmentCategoryDto>>>> GetCategories(
        [FromQuery] TreatmentCategorySearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await catalogService.GetCategoriesAsync(User.GetClinicId(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TreatmentCategoryDto>>.Ok(result));
    }

    [HttpGet("{categoryId:long}")]
    public async Task<ActionResult<ApiResponse<TreatmentCategoryDto>>> GetCategory(
        ulong categoryId,
        CancellationToken cancellationToken)
    {
        var category = await catalogService.GetCategoryByIdAsync(User.GetClinicId(), categoryId, cancellationToken);

        if (category is null)
        {
            return NotFound(ApiResponse<TreatmentCategoryDto>.Fail("Treatment category not found."));
        }

        return Ok(ApiResponse<TreatmentCategoryDto>.Ok(category));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TreatmentCategoryDto>>> CreateCategory(
        [FromBody] CreateTreatmentCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await catalogService.CreateCategoryAsync(
            User.GetClinicId(), User.GetUserId(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetCategory),
            new { categoryId = category.CategoryId },
            ApiResponse<TreatmentCategoryDto>.Ok(category, "Treatment category created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPut("{categoryId:long}")]
    public async Task<ActionResult<ApiResponse<TreatmentCategoryDto>>> UpdateCategory(
        ulong categoryId,
        [FromBody] UpdateTreatmentCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await catalogService.UpdateCategoryAsync(
            User.GetClinicId(), User.GetUserId(), categoryId, request, cancellationToken);

        if (category is null)
        {
            return NotFound(ApiResponse<TreatmentCategoryDto>.Fail("Treatment category not found."));
        }

        return Ok(ApiResponse<TreatmentCategoryDto>.Ok(category, "Treatment category updated successfully."));
    }
}