namespace DentalClinic.API.Services.Interfaces;

/// <summary>
/// Stages audit records on the current DbContext so they are persisted atomically
/// with the mutation they describe (same SaveChanges / transaction).
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Stages an audit log entry. Call <c>SaveChangesAsync</c> afterwards to persist it.
    /// </summary>
    /// <param name="actorUserId">Authenticated user performing the action.</param>
    /// <param name="clinicId">Clinic context of the action.</param>
    /// <param name="action">One of <see cref="Constants.AuditActions"/>.</param>
    /// <param name="entityName">One of <see cref="Constants.AuditEntities"/>.</param>
    /// <param name="entityId">Primary key of the affected entity, when known.</param>
    /// <param name="newData">Safe metadata snapshot after the change (never secrets).</param>
    /// <param name="oldData">Safe metadata snapshot before the change (never secrets).</param>
    void Record(
        ulong actorUserId,
        ulong clinicId,
        string action,
        string entityName,
        ulong? entityId = null,
        object? newData = null,
        object? oldData = null);
}