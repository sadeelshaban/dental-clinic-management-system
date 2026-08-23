using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Reports;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Extensions;

namespace DentalClinic.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly DentalClinicDbContext _db;

    public ReportsController(DentalClinicDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Daily financial summary for a date range
    /// </summary>
    [HttpGet("daily")]
    [Authorize(Roles = AppRoles.AdminOnly)]
    public async Task<IActionResult> GetDailyFinancialSummary([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var clinicId = User.GetClinicId();
        
        // Default to current month if no range specified
        if (!from.HasValue && !to.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            from = new DateOnly(today.Year, today.Month, 1);
            to = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        }
        else if (from.HasValue && !to.HasValue)
        {
            to = from.Value.AddDays(30);
        }
        else if (!from.HasValue && to.HasValue)
        {
            from = to.Value.AddDays(-30);
        }

        // Validate date range
        if (from > to)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid date range: 'from' must be less than or equal to 'to'."));
        }

        var data = await _db.DailyFinancialSummaries
            .Where(d => d.ClinicId == clinicId && d.FinancialDate >= from && d.FinancialDate <= to)
            .OrderBy(d => d.FinancialDate)
            .Select(d => new DailyFinancialSummaryDto(
                d.FinancialDate!.Value,
                d.Revenue ?? 0,
                d.Expenses ?? 0,
                d.NetProfit ?? 0
            ))
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<DailyFinancialSummaryDto>>.Ok(data));
    }

    /// <summary>
    /// Monthly financial summary
    /// </summary>
    [HttpGet("monthly")]
    [Authorize(Roles = AppRoles.AdminOnly)]
    public async Task<IActionResult> GetMonthlyFinancialSummary([FromQuery] int? year, [FromQuery] int? month)
    {
        var clinicId = User.GetClinicId();
        
        // Default to current month if not specified
        var now = DateTime.Now;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;
        
        var monthString = $"{targetYear}-{targetMonth:D2}-01";

        var data = await _db.MonthlyFinancialSummaries
            .Where(m => m.ClinicId == clinicId && m.Month == monthString)
            .Select(m => new MonthlyFinancialSummaryDto(
                m.Month!,
                m.Revenue ?? 0,
                m.Expenses ?? 0,
                m.NetProfit ?? 0,
                m.Patients ?? 0,
                m.Appointments ?? 0
            ))
            .FirstOrDefaultAsync();

        if (data == null)
        {
            return Ok(ApiResponse<MonthlyFinancialSummaryDto>.Ok(new MonthlyFinancialSummaryDto(
                monthString,
                0,
                0,
                0,
                0,
                0
            )));
        }

        return Ok(ApiResponse<MonthlyFinancialSummaryDto>.Ok(data));
    }

    /// <summary>
    /// Monthly performance comparison (current vs previous month)
    /// </summary>
    [HttpGet("comparison")]
    [Authorize(Roles = AppRoles.AdminOnly)]
    public async Task<IActionResult> GetMonthlyPerformanceComparison([FromQuery] int? year, [FromQuery] int? month)
    {
        var clinicId = User.GetClinicId();
        
        // Default to current month if not specified
        var now = DateTime.Now;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;
        
        var monthString = $"{targetYear}-{targetMonth:D2}-01";

        var data = await _db.MonthlyPerformanceComparisons
            .Where(m => m.ClinicId == clinicId && m.Month == monthString)
            .Select(m => new MonthlyPerformanceComparisonDto(
                m.Month!,
                m.Revenue ?? 0,
                m.Expenses ?? 0,
                m.NetProfit ?? 0,
                m.Patients ?? 0,
                m.Appointments ?? 0,
                m.PreviousMonthRevenue,
                m.PreviousMonthExpenses,
                m.PreviousMonthProfit,
                m.PreviousMonthPatients,
                m.PreviousMonthAppointments,
                m.RevenueChangePercent,
                m.ExpenseChangePercent,
                m.ProfitChangePercent,
                m.PatientChangePercent,
                m.AppointmentChangePercent
            ))
            .FirstOrDefaultAsync();

        if (data == null)
        {
            return Ok(ApiResponse<MonthlyPerformanceComparisonDto>.Ok(new MonthlyPerformanceComparisonDto(
                monthString,
                0,
                0,
                0,
                0,
                0,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            )));
        }

        return Ok(ApiResponse<MonthlyPerformanceComparisonDto>.Ok(data));
    }

    /// <summary>
    /// Patient directory with financial summaries
    /// </summary>
    [HttpGet("patient-directory")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> GetPatientDirectory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var clinicId = User.GetClinicId();
        
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _db.PatientDirectories
            .Where(p => p.ClinicId == clinicId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();
            query = query.Where(p => 
                p.PatientNumber.ToLower().Contains(searchTerm) ||
                p.FirstName.ToLower().Contains(searchTerm) ||
                p.LastName.ToLower().Contains(searchTerm) ||
                p.FullName.ToLower().Contains(searchTerm) ||
                (p.Phone != null && p.Phone.Contains(searchTerm)) ||
                (p.Email != null && p.Email.ToLower().Contains(searchTerm))
            );
        }

        // Apply active filter
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientDirectoryDto(
                p.PatientId,
                p.PatientNumber,
                p.FullName,
                p.Phone,
                p.Email,
                p.DateOfBirth,
                p.Gender,
                p.IsActive ?? true,
                p.TotalTreatments ?? 0,
                p.TotalPaid ?? 0,
                p.TotalRemaining ?? 0
            ))
            .ToListAsync();

        var pagedResult = new PagedResult<PatientDirectoryDto>
        {
            Items = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResult<PatientDirectoryDto>>.Ok(pagedResult));
    }

    /// <summary>
    /// Patients with outstanding balances
    /// </summary>
    [HttpGet("outstanding-balances")]
    [Authorize(Roles = AppRoles.ClinicalStaff)]
    public async Task<IActionResult> GetOutstandingBalances(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var clinicId = User.GetClinicId();
        
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _db.PatientDirectories
            .Where(p => p.ClinicId == clinicId && (p.TotalRemaining == null || p.TotalRemaining > 0));

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLower();
            query = query.Where(p => 
                p.PatientNumber.ToLower().Contains(searchTerm) ||
                p.FirstName.ToLower().Contains(searchTerm) ||
                p.LastName.ToLower().Contains(searchTerm) ||
                p.FullName.ToLower().Contains(searchTerm) ||
                (p.Phone != null && p.Phone.Contains(searchTerm)) ||
                (p.Email != null && p.Email.ToLower().Contains(searchTerm))
            );
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(p => p.TotalRemaining)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientDirectoryDto(
                p.PatientId,
                p.PatientNumber,
                p.FullName,
                p.Phone,
                p.Email,
                p.DateOfBirth,
                p.Gender,
                p.IsActive ?? true,
                p.TotalTreatments ?? 0,
                p.TotalPaid ?? 0,
                p.TotalRemaining ?? 0
            ))
            .ToListAsync();

        var pagedResult = new PagedResult<PatientDirectoryDto>
        {
            Items = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResult<PatientDirectoryDto>>.Ok(pagedResult));
    }
}
