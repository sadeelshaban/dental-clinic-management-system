using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Clinical encounters (visits). Distinct from appointments: an appointment is a
/// scheduling reservation; a visit is the actual clinical encounter. The schema has
/// no appointment↔visit FK, so no automatic linkage is created (documented).
///
/// Reads: all clinical staff. Writes: ADMIN + DOCTOR only — SECRETARY has no clinical
/// write permissions per the confirmed permission matrix. DOCTOR actors are always
/// scoped to their own profile; client-supplied doctorId is ignored for them.
/// No DELETE endpoint exists by design: clinical history must never be destroyed.
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class VisitsController(IVisitService visitService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<VisitListItemDto>>>> GetVisits(
        [FromQuery] VisitSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await visitService.GetVisitsAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<VisitListItemDto>>.Ok(result));
    }

    [HttpGet("{visitId:long}")]
    public async Task<ActionResult<ApiResponse<VisitDetailDto>>> GetVisit(
        ulong visitId,
        CancellationToken cancellationToken)
    {
        var visit = await visitService.GetVisitByIdAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), visitId, cancellationToken);

        if (visit is null)
        {
            return NotFound(ApiResponse<VisitDetailDto>.Fail("Visit not found."));
        }

        return Ok(ApiResponse<VisitDetailDto>.Ok(visit));
    }

    [Authorize(Roles = AppRoles.AdminOrDoctor)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VisitDetailDto>>> CreateVisit(
        [FromBody] CreateVisitRequest request,
        CancellationToken cancellationToken)
    {
        var visit = await visitService.CreateVisitAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetVisit),
            new { visitId = visit.VisitId },
            ApiResponse<VisitDetailDto>.Ok(visit, "Visit created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOrDoctor)]
    [HttpPut("{visitId:long}")]
    public async Task<ActionResult<ApiResponse<VisitDetailDto>>> UpdateVisit(
        ulong visitId,
        [FromBody] UpdateVisitRequest request,
        CancellationToken cancellationToken)
    {
        var visit = await visitService.UpdateVisitAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), visitId, request, cancellationToken);

        if (visit is null)
        {
            return NotFound(ApiResponse<VisitDetailDto>.Fail("Visit not found."));
        }

        return Ok(ApiResponse<VisitDetailDto>.Ok(visit, "Visit updated successfully."));
    }
}