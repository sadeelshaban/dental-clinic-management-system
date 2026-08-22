using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Auth;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.Extensions;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        if (result is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid email or password."));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var user = await authService.GetCurrentUserAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var success = await authService.ChangePasswordAsync(userId, request, cancellationToken);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.Fail("Current password is incorrect."));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Password updated successfully."));
    }
}
