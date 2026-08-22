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
public class ExpenseCategoriesController(IExpenseCategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseCategoryDto>>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await categoryService.GetCategoriesAsync(User.GetClinicId(), null, null, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpenseCategoryDto>>.Ok(result));
    }

    [HttpGet("{categoryId:long}")]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> GetCategory(ulong categoryId, CancellationToken cancellationToken)
    {
        var cat = await categoryService.GetCategoryByIdAsync(User.GetClinicId(), categoryId, cancellationToken);
        if (cat is null) return NotFound(ApiResponse<ExpenseCategoryDto>.Fail("Category not found."));
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(cat));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> CreateCategory([FromBody] CreateExpenseCategoryRequest request, CancellationToken cancellationToken)
    {
        var cat = await categoryService.CreateCategoryAsync(User.GetClinicId(), User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { categoryId = cat.CategoryId }, ApiResponse<ExpenseCategoryDto>.Ok(cat, "Category created."));
    }

    [Authorize(Roles = AppRoles.AdminOnly)]
    [HttpPut("{categoryId:long}")]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> UpdateCategory(ulong categoryId, [FromBody] UpdateExpenseCategoryRequest request, CancellationToken cancellationToken)
    {
        var cat = await categoryService.UpdateCategoryAsync(User.GetClinicId(), User.GetUserId(), categoryId, request, cancellationToken);
        if (cat is null) return NotFound(ApiResponse<ExpenseCategoryDto>.Fail("Category not found."));
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(cat, "Category updated."));
    }
}