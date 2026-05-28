using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/automations")]
public class AutomationController : ControllerBase
{
    private readonly ILogger<AutomationController> _logger;

    // Mock data storage
    private static List<Automation> _mockAutomations = new()
    {
        new Automation
        {
            Id = 1,
            Name = "Automação de Rega",
            Description = "Rega automática quando temperatura > 25°C",
            BatchId = 1,
            IsActive = true,
            TriggerCondition = "temperature > 25",
            Action = "activate_irrigation",
            OperationMode = "Automático",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new Automation
        {
            Id = 2,
            Name = "Automação de Ventilação",
            Description = "Ventilação automática quando humidade > 80%",
            BatchId = 1,
            IsActive = true,
            TriggerCondition = "humidity > 80",
            Action = "activate_ventilation",
            OperationMode = "Automático",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    };

    private static int _nextId = 3;

    public AutomationController(ILogger<AutomationController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Automation>> GetAll()
    {
        _logger.LogInformation("Obtendo todas as regras de automação");
        return Ok(_mockAutomations);
    }

    [HttpPost]
    public ActionResult<Automation> Create([FromBody] Automation automation)
    {
        if (automation == null)
        {
            _logger.LogWarning("Tentativa de criar automação com null");
            return BadRequest(new { message = "Automação não pode ser vazia" });
        }

        if (string.IsNullOrWhiteSpace(automation.Name))
        {
            return BadRequest(new { message = "Nome é obrigatório" });
        }

        if (automation.BatchId <= 0)
        {
            return BadRequest(new { message = "BatchId inválido" });
        }

        automation.Id = _nextId++;
        automation.CreatedAt = DateTime.UtcNow;
        automation.UpdatedAt = DateTime.UtcNow;

        _mockAutomations.Add(automation);
        _logger.LogInformation("Automação criada com ID: {id}", automation.Id);

        return CreatedAtAction(nameof(GetById), new { id = automation.Id }, automation);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Automation> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter automação com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var automation = _mockAutomations.FirstOrDefault(a => a.Id == id);
        if (automation == null)
        {
            _logger.LogWarning("Automação com ID {id} não encontrada", id);
            return NotFound();
        }

        return Ok(automation);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Automation> Update([FromRoute] int id, [FromBody] Automation updated)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        var automation = _mockAutomations.FirstOrDefault(a => a.Id == id);
        if (automation == null)
        {
            _logger.LogWarning("Automação com ID {id} não encontrada para atualização", id);
            return NotFound();
        }

        automation.Name = updated.Name;
        automation.Description = updated.Description;
        automation.BatchId = updated.BatchId;
        automation.IsActive = updated.IsActive;
        automation.TriggerCondition = updated.TriggerCondition;
        automation.Action = updated.Action;
        automation.OperationMode = updated.OperationMode;
        automation.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Automação com ID {id} atualizada", id);
        return Ok(automation);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        var automation = _mockAutomations.FirstOrDefault(a => a.Id == id);
        if (automation == null)
        {
            _logger.LogWarning("Automação com ID {id} não encontrada para deleção", id);
            return NotFound();
        }

        _mockAutomations.Remove(automation);
        _logger.LogInformation("Automação com ID {id} deletada", id);
        return NoContent();
    }
}
