using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Auth;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class AuthService(
    DentalClinicDbContext dbContext,
    ITokenService tokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Email.ToLower() == normalizedEmail && u.IsActive != false,
                cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var trackedUser = await dbContext.Users
            .FirstAsync(u => u.UserId == user.UserId, cancellationToken);

        trackedUser.LastLoginAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var userDto = MapToDto(trackedUser);
        return tokenService.BuildLoginResponse(userDto);
    }

    public async Task<UserDto?> GetCurrentUserAsync(
        ulong userId,
        CancellationToken cancellationToken = default)
    {
        // Deactivated users must not be able to act on the system even while a
        // previously issued token has not expired yet.
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.UserId == userId && u.IsActive != false,
                cancellationToken);

        return user is null ? null : MapToDto(user);
    }

    public async Task<bool> ChangePasswordAsync(
        ulong userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static UserDto MapToDto(Models.User user) =>
        new()
        {
            UserId = user.UserId,
            ClinicId = user.ClinicId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone,
            IsActive = user.IsActive != false
        };
}
