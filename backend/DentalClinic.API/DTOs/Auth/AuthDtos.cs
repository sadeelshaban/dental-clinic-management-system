using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public ulong UserId { get; set; }

    public ulong ClinicId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
