using System.Text.Json;
using DentalClinic.API.Data;
using DentalClinic.API.Services.Interfaces;

namespace DentalClinic.API.Services.Implementations;

/// <summary>
/// Writes to the existing audit_logs table. Entries are staged on the caller's DbContext
/// and persisted by the caller's SaveChangesAsync, so audit rows commit or roll back
/// together with the mutation they describe.
///
/// SECURITY: callers must only pass safe metadata (ids, names, roles, flags).
/// Passwords, password hashes, tokens, JWTs, and secrets must NEVER be passed here.
/// </summary>
public class AuditService(
    DentalClinicDbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Record(
        ulong actorUserId,
        ulong clinicId,
        string action,
        string entityName,
        ulong? entityId = null,
        object? newData = null,
        object? oldData = null)
    {
        var httpContext = httpContextAccessor.HttpContext;

        dbContext.AuditLogs.Add(new Models.AuditLog
        {
            ClinicId = clinicId,
            UserId = actorUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = Serialize(oldData),
            NewValues = Serialize(newData),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(httpContext?.Request.Headers.UserAgent.ToString(), 2000),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string? Serialize(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value, JsonOptions);

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? value : value[..Math.Min(value.Length, maxLength)];
}