using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Billing;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Patients;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class PatientsController(
    IPatientService patientService,
    IPaymentService paymentService) : ControllerBase
{
    /// <summary>
    /// Patient financial statement (Phase 4): totals from the existing
    /// patient_financial_summary view, per-treatment lines from
    /// patient_treatment_financials, plus payment history. Clinic-scoped; DOCTOR
    /// actors see only their own treatment lines/payments within the statement.
    /// </summary>
    [HttpGet("{patientId:long}/financial")]
    public async Task<ActionResult<ApiResponse<PatientFinancialStatementDto>>> GetFinancialStatement(
        ulong patientId,
        CancellationToken cancellationToken)
    {
        var statement = await paymentService.GetPatientFinancialStatementAsync(
            User.GetClinicId(), User.GetUserId(), User.GetRole(), patientId, cancellationToken);

        if (statement is null)
        {
            return NotFound(ApiResponse<PatientFinancialStatementDto>.Fail("Patient not found."));
        }

        return Ok(ApiResponse<PatientFinancialStatementDto>.Ok(statement));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PatientListItemDto>>>> GetPatients(
        [FromQuery] PatientSearchQuery query,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var result = await patientService.GetPatientsAsync(clinicId, query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PatientListItemDto>>.Ok(result));
    }

    [HttpGet("{patientId:long}")]
    public async Task<ActionResult<ApiResponse<PatientDetailDto>>> GetPatient(
        ulong patientId,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var patient = await patientService.GetPatientByIdAsync(clinicId, patientId, cancellationToken);

        if (patient is null)
        {
            return NotFound(ApiResponse<PatientDetailDto>.Fail("Patient not found."));
        }

        return Ok(ApiResponse<PatientDetailDto>.Ok(patient));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PatientDetailDto>>> CreatePatient(
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var patient = await patientService.CreatePatientAsync(clinicId, request, cancellationToken);
        return CreatedAtAction(
            nameof(GetPatient),
            new { patientId = patient.PatientId },
            ApiResponse<PatientDetailDto>.Ok(patient, "Patient created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpPut("{patientId:long}")]
    public async Task<ActionResult<ApiResponse<PatientDetailDto>>> UpdatePatient(
        ulong patientId,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var patient = await patientService.UpdatePatientAsync(clinicId, patientId, request, cancellationToken);

        if (patient is null)
        {
            return NotFound(ApiResponse<PatientDetailDto>.Fail("Patient not found."));
        }

        return Ok(ApiResponse<PatientDetailDto>.Ok(patient, "Patient updated successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOrSecretary)]
    [HttpDelete("{patientId:long}")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivatePatient(
        ulong patientId,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var success = await patientService.DeactivatePatientAsync(clinicId, patientId, cancellationToken);

        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Patient not found."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Patient deactivated successfully."));
    }
}
