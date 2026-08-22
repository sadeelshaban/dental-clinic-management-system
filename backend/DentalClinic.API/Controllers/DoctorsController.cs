using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Doctors;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// Doctor directory. Read access for all clinical staff (needed for scheduling and
/// clinical workflows); profile updates are ADMIN-only.
///
/// Doctors are created through POST /api/users with role=DOCTOR (the doctors table
/// requires a linked user account), so there is intentionally no standalone
/// POST /api/doctors endpoint. Activation/deactivation is driven by the linked
/// user account (POST /api/users/{id}/activate|deactivate), which keeps the two
/// flags in sync.
/// </summary>
[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class DoctorsController(IDoctorService doctorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DoctorListItemDto>>>> GetDoctors(
        [FromQuery] DoctorSearchQuery query,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var result = await doctorService.GetDoctorsAsync(clinicId, query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DoctorListItemDto>>.Ok(result));
    }

    [HttpGet("{doctorId:long}")]
    public async Task<ActionResult<ApiResponse<DoctorDetailDto>>> GetDoctor(
        ulong doctorId,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var doctor = await doctorService.GetDoctorByIdAsync(clinicId, doctorId, cancellationToken);

        if (doctor is null)
        {
            return NotFound(ApiResponse<DoctorDetailDto>.Fail("Doctor not found."));
        }

        return Ok(ApiResponse<DoctorDetailDto>.Ok(doctor));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPut("{doctorId:long}")]
    public async Task<ActionResult<ApiResponse<DoctorDetailDto>>> UpdateDoctor(
        ulong doctorId,
        [FromBody] UpdateDoctorRequest request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var actorUserId = User.GetUserId();
        var doctor = await doctorService.UpdateDoctorAsync(
            clinicId, actorUserId, doctorId, request, cancellationToken);

        if (doctor is null)
        {
            return NotFound(ApiResponse<DoctorDetailDto>.Fail("Doctor not found."));
        }

        return Ok(ApiResponse<DoctorDetailDto>.Ok(doctor, "Doctor updated successfully."));
    }
}