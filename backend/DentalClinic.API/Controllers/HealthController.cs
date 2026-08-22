using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(DentalClinicDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> Get(CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        if (!canConnect)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail("MariaDB connection failed."));
        }

        var clinicCount = await dbContext.Clinics.CountAsync(cancellationToken);

        return Ok(ApiResponse<object>.Ok(new
        {
            status = "healthy",
            database = "dental_clinic_db",
            provider = "MariaDB 10.4",
            clinics = clinicCount
        }));
    }
}
