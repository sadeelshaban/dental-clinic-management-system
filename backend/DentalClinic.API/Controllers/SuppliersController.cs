using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Expenses;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

[Authorize(Roles = AppRoles.ClinicalStaff)]
[ApiController]
[Route("api/[controller]")]
public class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierDto>>>> GetSuppliers(
        [FromQuery] SupplierSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await supplierService.GetSuppliersAsync(User.GetClinicId(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<SupplierDto>>.Ok(result));
    }

    [HttpGet("{supplierId:long}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(
        ulong supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierService.GetSupplierByIdAsync(User.GetClinicId(), supplierId, cancellationToken);
        if (supplier is null)
        {
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found."));
        }

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierService.CreateSupplierAsync(User.GetClinicId(), User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(
            nameof(GetSupplier),
            new { supplierId = supplier.SupplierId },
            ApiResponse<SupplierDto>.Ok(supplier, "Supplier created successfully."));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPut("{supplierId:long}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(
        ulong supplierId,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await supplierService.UpdateSupplierAsync(User.GetClinicId(), User.GetUserId(), supplierId, request, cancellationToken);
        if (supplier is null)
        {
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found."));
        }

        return Ok(ApiResponse<SupplierDto>.Ok(supplier, "Supplier updated successfully."));
    }

    [HttpGet("{supplierId:long}/statement")]
    public async Task<ActionResult<ApiResponse<SupplierFinancialStatementDto>>> GetSupplierStatement(
        ulong supplierId,
        CancellationToken cancellationToken)
    {
        var stmt = await supplierService.GetSupplierFinancialStatementAsync(User.GetClinicId(), supplierId, cancellationToken);
        if (stmt is null)
        {
            return NotFound(ApiResponse<SupplierFinancialStatementDto>.Fail("Supplier not found."));
        }

        return Ok(ApiResponse<SupplierFinancialStatementDto>.Ok(stmt));
    }
}