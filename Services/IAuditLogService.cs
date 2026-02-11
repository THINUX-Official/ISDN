namespace ISDN.Services
{
    /// <summary>
    /// Interface for audit logging service
    /// </summary>
    public interface IAuditLogService
    {
        Task LogActionAsync(int userId, string action, string? entityType, int? entityId, string? description, string? ipAddress);
        Task<IEnumerable<Models.AuditLog>> GetAuditLogsAsync(int? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
