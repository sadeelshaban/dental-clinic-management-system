using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Clinical;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Patient treatments = treatments actually performed/recorded for a patient.
/// Reads: all clinical staff. Writes: ADMIN + DOCTOR only (SECRETARY records
/// payments in Phase 4, not clinical entries). DOCTOR actors are always scoped to
/// their own profile; client-supplied doctorId is ignored for them. Name and unit
/// price are snapshotted at creation; catalog edits never alter history. No DELETE
/// endpoint exists by design: billing history must never be destroyed (voiding is a
/// Phase 4 payment concern).
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class PatientTreatmentsController(IPatientTreatmentService patientTreatmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PatientTreatmentListItemDto>>>> GetPatientTreatments(
        [FromQuery] PatientTreatmentSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await patientTreatmentService.GetPatientTreatmentsAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PatientTreatmentListItemDto>>.Ok(result));
    }

    [HttpGet("{patientTreatmentId:long}")]
    public async Task<ActionResult<ApiResponse<PatientTreatmentDetailDto>>> GetPatientTreatment(
        ulong patientTreatmentId,
        CancellationToken cancellationToken)
    {
        var treatment = await patientTreatmentService.GetPatientTreatmentByIdAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), patientTreatmentId, cancellationToken);

        if (treatment is null)
        {
            return NotFound(ApiResponse<PatientTreatmentDetailDto>.Fail("Patient treatment not found."));
        }

        return Ok(ApiResponse<PatientTreatmentDetailDto>.Ok(treatment));
    }

    [Authorize(Roles = AppRoles.AdminOrDoctor)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PatientTreatmentDetailDto>>> CreatePatientTreatment(
        [FromBody] CreatePatientTreatmentRequest request,
        CancellationToken cancellationToken)
    {
        var treatment = await patientTreatmentService.CreatePatientTreatmentAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), request, cancellationToken);

        return CreatedAtAction(
            nameof(GetPatientTreatment),
            new { patientTreatmentId = treatment.PatientTreatmentId },
            ApiResponse<PatientTreatmentDetailDto>.Ok(treatment, "Patient treatment recorded successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOrDoctor)]
    [HttpPut("{patientTreatmentId:long}")]
    public async Task<ActionResult<ApiResponse<PatientTreatmentDetailDto>>> UpdatePatientTreatment(
        ulong patientTreatmentId,
        [FromBody] UpdatePatientTreatmentRequest request,
        CancellationToken cancellationToken)
    {
        var treatment = await patientTreatmentService.UpdatePatientTreatmentAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), patientTreatmentId, request, cancellationToken);

        if (treatment is null)
        {
            return NotFound(ApiResponse<PatientTreatmentDetailDto>.Fail("Patient treatment not found."));
        }

        return Ok(ApiResponse<PatientTreatmentDetailDto>.Ok(treatment, "Patient treatment updated successfully."));
    }
}