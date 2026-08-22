using System.Security.Claims;
using DentalClinic.API.Constants;

namespace DentalClinic.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static ulong GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypesCustom.UserId)
            ?? throw new UnauthorizedAccessException("User id claim is missing.");

        return ulong.Parse(value);
    }

    public static ulong GetClinicId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypesCustom.ClinicId)
            ?? throw new UnauthorizedAccessException("Clinic id claim is missing.");

        return ulong.Parse(value);
    }

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}
