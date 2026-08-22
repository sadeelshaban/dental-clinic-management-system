using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Users;

namespace DentalClinic.API.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(
        ulong clinicId,
        UserSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <returns>The user detail, or null when the user does not exist in the clinic.</returns>
    Task<UserDetailDto?> GetUserByIdAsync(
        ulong clinicId,
        ulong userId,
        CancellationToken cancellationToken = default);

    /// <exception cref="Common.BusinessRuleException">Invalid role, duplicate email, or persistence conflict.</exception>
    Task<UserDetailDto> CreateUserAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    /// <returns>The updated detail, or null when the user does not exist in the clinic.</returns>
    /// <exception cref="Common.BusinessRuleException">Duplicate email, invalid role, unsafe role change, last-admin protection.</exception>
    Task<UserDetailDto?> UpdateUserAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Activates or deactivates a user (and syncs the linked doctor profile).</summary>
    /// <returns>false when the user does not exist in the clinic.</returns>
    /// <exception cref="Common.BusinessRuleException">Self-deactivation or last-active-admin protection.</exception>
    Task<bool> SetUserActiveStatusAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong userId,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>Sets a new password on behalf of an ADMIN. The password itself is never logged.</summary>
    /// <returns>false when the user does not exist in the clinic.</returns>
    Task<bool> ResetPasswordAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong userId,
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}