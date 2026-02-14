using Microsoft.EntityFrameworkCore;
using ISDN.Data;
using ISDN.Models;

namespace ISDN.Services
{
    /// <summary>
    /// Service for logging important actions in the system
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IsdnDbContext _context;

        public AuditLogService(IsdnDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Log an action to the audit_logs table
        /// </summary>
        public async Task LogActionAsync(
            int userId,
            string action,
            string? entityType,
            int? entityId,
            string? description,
            string? ipAddress)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Description = description,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to prevent breaking the main operation
                Console.WriteLine($"Audit log error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieve audit logs with optional filters
        /// </summary>
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            int? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(1000) // Limit to 1000 records
                .ToListAsync();
        }
    }
}
