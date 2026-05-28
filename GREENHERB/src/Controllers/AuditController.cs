using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditLogService auditLogService, ILogger<AuditController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAll()
    {
        _logger.LogInformation("Obtendo todos os logs de auditoria");
        var logs = await _auditLogService.GetAllAsync();
        return Ok(logs);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuditLog>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter log com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var log = await _auditLogService.GetByIdAsync(id.ToString());
        if (log == null)
        {
            _logger.LogWarning("Log de auditoria com ID {id} não encontrado", id);
            return NotFound(new { message = "Log de auditoria não encontrado" });
        }

        return Ok(log);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetByUserId([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new { message = "ID do utilizador inválido" });
        }

        _logger.LogInformation("Obtendo logs de auditoria do utilizador: {userId}", userId);
        var logs = await _auditLogService.GetByUserIdAsync(userId);
        return Ok(logs);
    }

    [HttpGet("entity/{entityType}/{entityId:int}")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetByEntity([FromRoute] string entityType, [FromRoute] int entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType) || entityId <= 0)
        {
            return BadRequest(new { message = "Tipo de entidade e ID são obrigatórios" });
        }

        _logger.LogInformation("Obtendo logs de auditoria da entidade {type}: {id}", entityType, entityId);
        var logs = await _auditLogService.GetByEntityAsync(entityType, entityId.ToString());
        return Ok(logs);
    }

    [HttpGet("operation/{operationType}")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetByOperationType([FromRoute] string operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
        {
            return BadRequest(new { message = "Tipo de operação é obrigatório" });
        }

        _logger.LogInformation("Obtendo logs de auditoria da operação: {type}", operationType);
        var logs = await _auditLogService.GetByOperationTypeAsync(operationType);
        return Ok(logs);
    }

    [HttpGet("range")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Data inicial deve ser anterior à data final" });
        }

        _logger.LogInformation("Obtendo logs de auditoria entre {start} e {end}", startDate, endDate);
        var logs = await _auditLogService.GetByDateRangeAsync(startDate, endDate);
        return Ok(logs);
    }

    [HttpDelete("older-than")]
    public async Task<ActionResult> DeleteOldLogs([FromQuery] DateTime beforeDate)
    {
        _logger.LogInformation("Deletando logs de auditoria anteriores a: {date}", beforeDate);
        var deletedCount = await _auditLogService.DeleteOldLogsAsync(beforeDate);
        return Ok(new { message = $"{deletedCount} logs de auditoria foram deletados" });
    }
}
