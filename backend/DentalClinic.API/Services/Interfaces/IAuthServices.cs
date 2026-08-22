using DentalClinic.API.DTOs.Auth;

namespace DentalClinic.API.Services.Interfaces;

public interface ITokenService
{
    LoginResponse BuildLoginResponse(UserDto user);
}

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> GetCurrentUserAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(
        ulong userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
