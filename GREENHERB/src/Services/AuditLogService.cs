using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class AuditLogService : IAuditLogService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<AuditLogService>? _logger;
    private bool _useMock = true;

    public AuditLogService(AppDbContext? context = null, ILogger<AuditLogService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todos os logs de auditoria");
        if (_useMock)
            return MockDataProvider.GetAllAuditLogs();

        return await _context!.AuditLogs.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<AuditLog?> GetByIdAsync(string id)
    {
        _logger?.LogInformation("Obtendo log de auditoria com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetAuditLogById(id);

        return await _context!.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
    {
        _logger?.LogInformation("Obtendo logs de auditoria do utilizador: {userId}", userId);
        if (_useMock)
            return MockDataProvider.GetAuditLogsByUserId(userId);

        return await _context!.AuditLogs.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        _logger?.LogInformation("Obtendo logs de auditoria da entidade {entityType}: {entityId}", entityType, entityId);
        if (_useMock)
            return MockDataProvider.GetAuditLogsByEntity(entityType, entityId);

        return await _context!.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByOperationTypeAsync(string operationType)
    {
        _logger?.LogInformation("Obtendo logs de auditoria da operação: {type}", operationType);
        if (_useMock)
            return MockDataProvider.GetAuditLogsByOperationType(operationType);

        return await _context!.AuditLogs.Where(a => a.OperationType == operationType).OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger?.LogInformation("Obtendo logs de auditoria entre {start} e {end}", startDate, endDate);
        if (_useMock)
            return MockDataProvider.GetAuditLogsByDateRange(startDate, endDate);

        return await _context!.AuditLogs
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<AuditLog> LogOperationAsync(int userId, string operationType, string entityType, string entityId, string description, string? oldValues = null, string? newValues = null, string? ipAddress = null)
    {
        _logger?.LogInformation("Registrando operação de auditoria: {operation} em {entity} {id}", operationType, entityType, entityId);
        
        if (_useMock)
        {
            var result = MockDataProvider.LogOperation(userId, operationType, entityType, entityId, description, oldValues, newValues, ipAddress);
            _logger?.LogInformation("Operação de auditoria registrada com sucesso. ID: {id}", result.Id);
            return result;
        }

        var auditLog = new AuditLog
        {
            UserId = userId,
            OperationType = operationType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress
        };

        _context!.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Operação de auditoria registrada com sucesso. ID: {id}", auditLog.Id);
        return auditLog;
    }

    public async Task<int> DeleteOldLogsAsync(DateTime beforeDate)
    {
        _logger?.LogInformation("Deletando logs de auditoria anteriores a: {date}", beforeDate);
        
        if (_useMock)
        {
            var count = MockDataProvider.DeleteOldAuditLogs(beforeDate);
            _logger?.LogInformation("Foram deletados {count} logs de auditoria", count);
            return count;
        }

        var logsToDelete = await _context!.AuditLogs.Where(a => a.CreatedAt < beforeDate).ToListAsync();
        
        _context.AuditLogs.RemoveRange(logsToDelete);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Foram deletados {count} logs de auditoria", logsToDelete.Count);
        return logsToDelete.Count;
    }
}
