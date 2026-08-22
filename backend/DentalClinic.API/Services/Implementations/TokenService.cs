using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DentalClinic.API.Configuration;
using DentalClinic.API.Constants;
using DentalClinic.API.DTOs.Auth;
using DentalClinic.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DentalClinic.API.Services.Implementations;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public LoginResponse BuildLoginResponse(UserDto user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
        var token = CreateToken(user, expiresAt);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = user
        };
    }

    private string CreateToken(UserDto user, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypesCustom.UserId, user.UserId.ToString()),
            new(ClaimTypesCustom.ClinicId, user.ClinicId.ToString()),
            new(ClaimTypesCustom.FullName, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
