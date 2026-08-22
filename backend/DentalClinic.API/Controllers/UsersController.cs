using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Users;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

/// <summary>
/// ADMIN-only user management. All operations are scoped to the authenticated
/// admin's clinic; clinic_id is always derived from the JWT, never from the client.
/// </summary>
[Authorize(Roles = AppRoles.AdminOnly)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserListItemDto>>>> GetUsers(
        [FromQuery] UserSearchQuery query,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var result = await userService.GetUsersAsync(clinicId, query, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserListItemDto>>.Ok(result));
    }

    [HttpGet("{userId:long}")]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetUser(
        ulong userId,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var user = await userService.GetUserByIdAsync(clinicId, userId, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<UserDetailDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<UserDetailDto>.Ok(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var actorUserId = User.GetUserId();
        var user = await userService.CreateUserAsync(
            clinicId, actorUserId, request, cancellationToken);

        return CreatedAtAction(
            nameof(GetUser),
            new { userId = user.UserId },
            ApiResponse<UserDetailDto>.Ok(user, "User created successfully."));
    }

    [HttpPut("{userId:long}")]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> UpdateUser(
        ulong userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var actorUserId = User.GetUserId();
        var user = await userService.UpdateUserAsync(
            clinicId, actorUserId, userId, request, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<UserDetailDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<UserDetailDto>.Ok(user, "User updated successfully."));
    }

    [HttpPost("{userId:long}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(
        ulong userId,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var actorUserId = User.GetUserId();
        var success = await userService.SetUserActiveStatusAsync(
            clinicId, actorUserId, userId, isActive: true, cancellationToken);

        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("User not found."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "User activated successfully."));
    }

    [HttpPost("{userId:long}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(
        ulong userId,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var actorUserId = User.GetUserId();
        var success = await userService.SetUserActiveStatusAsync(
            clinicId, actorUserId, userId, isActive: false, cancellationToken);

        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("User not found."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "User deactivated successfully."));
    }

    /// <summary>ADMIN sets a new password for a user. The password is never returned or logged.</summary>
    [HttpPost("{userId:long}/reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        ulong userId,
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var clinicId = User.GetClinicId();
        var actorUserId = User.GetUserId();
        var success = await userService.ResetPasswordAsync(
            clinicId, actorUserId, userId, request, cancellationToken);

        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("User not found."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Password reset successfully."));
    }
}