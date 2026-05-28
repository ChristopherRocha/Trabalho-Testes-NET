using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IAlertService alertService, ILogger<AlertsController> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Alert>>> GetAll()
    {
        _logger.LogInformation("Obtendo todos os alertas");
        var alerts = await _alertService.GetAllAsync();
        return Ok(alerts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Alert>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter alerta com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var alert = await _alertService.GetByIdAsync(id);
        if (alert == null)
        {
            _logger.LogWarning("Alerta com ID {id} não encontrado", id);
            return NotFound(new { message = "Alerta não encontrado" });
        }

        return Ok(alert);
    }

    [HttpGet("type/{alertType}")]
    public async Task<ActionResult<IEnumerable<Alert>>> GetByType([FromRoute] string alertType)
    {
        if (string.IsNullOrWhiteSpace(alertType))
        {
            return BadRequest(new { message = "Tipo de alerta é obrigatório" });
        }

        var alerts = await _alertService.GetByTypeAsync(alertType);
        return Ok(alerts);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<Alert>>> GetByStatus([FromRoute] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(new { message = "Status é obrigatório" });
        }

        var alerts = await _alertService.GetByStatusAsync(status);
        return Ok(alerts);
    }

    [HttpGet("resource/{resourceId:int}")]
    public async Task<ActionResult<IEnumerable<Alert>>> GetByResourceId([FromRoute] int resourceId)
    {
        if (resourceId <= 0)
        {
            return BadRequest(new { message = "ID do recurso inválido" });
        }

        var alerts = await _alertService.GetByResourceIdAsync(resourceId);
        return Ok(alerts);
    }

    [HttpPost]
    public async Task<ActionResult<Alert>> Create([FromBody] Alert alert)
    {
        var validationError = ValidateAlert(alert);
        if (validationError != null)
        {
            _logger.LogWarning("Tentativa de criar alerta com validação falha: {error}", validationError);
            return BadRequest(new { message = validationError });
        }

        _logger.LogInformation("Criando novo alerta: {title}", alert.Title);
        var created = await _alertService.CreateAsync(alert);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}/resolve")]
    public async Task<ActionResult<Alert>> Resolve([FromRoute] int id, [FromBody] ResolveAlertRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        if (string.IsNullOrWhiteSpace(request?.Resolution))
        {
            return BadRequest(new { message = "Justificação é obrigatória" });
        }

        _logger.LogInformation("Resolvendo alerta com ID: {id}", id);
        var resolved = await _alertService.ResolveAsync(id, request.ResolvedByUserId, request.Resolution);
        if (resolved == null)
        {
            return NotFound(new { message = "Alerta não encontrado" });
        }

        return Ok(resolved);
    }

    [HttpPut("{id:int}/ignore")]
    public async Task<ActionResult<Alert>> Ignore([FromRoute] int id, [FromBody] IgnoreAlertRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        if (string.IsNullOrWhiteSpace(request?.Justification))
        {
            return BadRequest(new { message = "Justificação é obrigatória" });
        }

        _logger.LogInformation("Ignorando alerta com ID: {id}", id);
        var ignored = await _alertService.IgnoreAsync(id, request.IgnoredByUserId, request.Justification);
        if (ignored == null)
        {
            return NotFound(new { message = "Alerta não encontrado" });
        }

        return Ok(ignored);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Deletando alerta com ID: {id}", id);
        var deleted = await _alertService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Alerta não encontrado" });
        }

        return NoContent();
    }

    private static string? ValidateAlert(Alert alert)
    {
        if (alert == null)
        {
            return "Dados do alerta são obrigatórios";
        }

        if (string.IsNullOrWhiteSpace(alert.Title))
        {
            return "Título do alerta é obrigatório";
        }

        if (string.IsNullOrWhiteSpace(alert.AlertType))
        {
            return "Tipo de alerta é obrigatório";
        }

        return null;
    }
}

public class ResolveAlertRequest
{
    public string Resolution { get; set; } = string.Empty;
    public int ResolvedByUserId { get; set; }
}

public class IgnoreAlertRequest
{
    public string Justification { get; set; } = string.Empty;
    public int IgnoredByUserId { get; set; }
}
