using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Treatment catalog (reusable service definitions). Reads: all clinical staff.
/// Writes: ADMIN only. DefaultPrice changes affect ONLY future patient treatments —
/// historical records keep their own immutable snapshot. Catalog items are soft-
/// deactivated via PUT (isActive=false), never deleted, because historical patient
/// treatments reference them (FK ON DELETE SET NULL would silently detach history).
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class TreatmentsController(ITreatmentCatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TreatmentListItemDto>>>> GetTreatments(
        [FromQuery] TreatmentSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await catalogService.GetTreatmentsAsync(User.GetClinicId(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TreatmentListItemDto>>.Ok(result));
    }

    [HttpGet("{treatmentId:long}")]
    public async Task<ActionResult<ApiResponse<TreatmentDetailDto>>> GetTreatment(
        ulong treatmentId,
        CancellationToken cancellationToken)
    {
        var treatment = await catalogService.GetTreatmentByIdAsync(User.GetClinicId(), treatmentId, cancellationToken);

        if (treatment is null)
        {
            return NotFound(ApiResponse<TreatmentDetailDto>.Fail("Treatment not found."));
        }

        return Ok(ApiResponse<TreatmentDetailDto>.Ok(treatment));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TreatmentDetailDto>>> CreateTreatment(
        [FromBody] CreateTreatmentRequest request,
        CancellationToken cancellationToken)
    {
        var treatment = await catalogService.CreateTreatmentAsync(
            User.GetClinicId(), User.GetUserId(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetTreatment),
            new { treatmentId = treatment.TreatmentId },
            ApiResponse<TreatmentDetailDto>.Ok(treatment, "Treatment created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPut("{treatmentId:long}")]
    public async Task<ActionResult<ApiResponse<TreatmentDetailDto>>> UpdateTreatment(
        ulong treatmentId,
        [FromBody] UpdateTreatmentRequest request,
        CancellationToken cancellationToken)
    {
        var treatment = await catalogService.UpdateTreatmentAsync(
            User.GetClinicId(), User.GetUserId(), treatmentId, request, cancellationToken);

        if (treatment is null)
        {
            return NotFound(ApiResponse<TreatmentDetailDto>.Fail("Treatment not found."));
        }

        return Ok(ApiResponse<TreatmentDetailDto>.Ok(treatment, "Treatment updated successfully."));
    }
}