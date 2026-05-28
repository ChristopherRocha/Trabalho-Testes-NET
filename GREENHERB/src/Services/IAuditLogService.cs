using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IAuditLogService
{
    /// <summary>
    /// Obtém todos os logs de auditoria.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetAllAsync();

    /// <summary>
    /// Obtém um log de auditoria pelo ID.
    /// </summary>
    Task<AuditLog?> GetByIdAsync(string id);

    /// <summary>
    /// Obtém logs de auditoria por utilizador.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Obtém logs de auditoria de uma entidade.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);

    /// <summary>
    /// Obtém logs de auditoria por tipo de operação.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByOperationTypeAsync(string operationType);

    /// <summary>
    /// Obtém logs de um período.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Registra uma operação de auditoria.
    /// </summary>
    Task<AuditLog> LogOperationAsync(int userId, string operationType, string entityType, string entityId, string description, string? oldValues = null, string? newValues = null, string? ipAddress = null);

    /// <summary>
    /// Deleta logs antigos (anterior a uma data).
    /// </summary>
    Task<int> DeleteOldLogsAsync(DateTime beforeDate);
}
