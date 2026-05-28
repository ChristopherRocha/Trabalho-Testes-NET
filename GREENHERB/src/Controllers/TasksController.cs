using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async System.Threading.Tasks.Task<ActionResult<IEnumerable<OperationalTask>>> GetAll()
    {
        _logger.LogInformation("Obtendo todas as tarefas");
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async System.Threading.Tasks.Task<ActionResult<OperationalTask>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter tarefa com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
        {
            _logger.LogWarning("Tarefa com ID {id} não encontrada", id);
            return NotFound(new { message = "Tarefa não encontrada" });
        }

        return Ok(task);
    }

    [HttpGet("batch/{batchId:int}")]
    public async System.Threading.Tasks.Task<ActionResult<IEnumerable<OperationalTask>>> GetByBatchId([FromRoute] int batchId)
    {
        if (batchId <= 0)
        {
            return BadRequest(new { message = "ID do lote inválido" });
        }

        var tasks = await _taskService.GetByBatchIdAsync(batchId);
        return Ok(tasks);
    }

    [HttpGet("status/{status}")]
    public async System.Threading.Tasks.Task<ActionResult<IEnumerable<OperationalTask>>> GetByStatus([FromRoute] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(new { message = "Status é obrigatório" });
        }

        var tasks = await _taskService.GetByStatusAsync(status);
        return Ok(tasks);
    }

    [HttpPost]
    public async System.Threading.Tasks.Task<ActionResult<OperationalTask>> Create([FromBody] TaskRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Tentativa de criar tarefa com null");
            return BadRequest(new { message = "Dados da tarefa são obrigatórios" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Nome da tarefa é obrigatório" });
        }

        if (request.BatchId <= 0)
        {
            return BadRequest(new { message = "ID do lote é obrigatório" });
        }

        if (request.ScheduledDate == default)
        {
            return BadRequest(new { message = "Data agendada é obrigatória" });
        }

        var task = new OperationalTask
        {
            Name = request.Name,
            BatchId = request.BatchId,
            TaskType = request.TaskType,
            Description = request.Description,
            ScheduledDate = request.ScheduledDate,
            Status = request.Status,
            AssignedUserId = request.AssignedUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Criando nova tarefa: {name}", task.Name);
        var created = await _taskService.CreateAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async System.Threading.Tasks.Task<ActionResult<OperationalTask>> Update([FromRoute] int id, [FromBody] OperationalTask updated)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        var validationError = ValidateTask(updated);
        if (validationError != null)
        {
            _logger.LogWarning("Tentativa de atualizar tarefa com validação falha: {error}", validationError);
            return BadRequest(new { message = validationError });
        }

        _logger.LogInformation("Atualizando tarefa com ID: {id}", id);
        var updatedTask = await _taskService.UpdateAsync(id, updated);
        if (updatedTask == null)
        {
            return NotFound(new { message = "Tarefa não encontrada" });
        }

        return Ok(updatedTask);
    }

    [HttpDelete("{id:int}")]
    public async System.Threading.Tasks.Task<ActionResult> Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Deletando tarefa com ID: {id}", id);
        var deleted = await _taskService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Tarefa não encontrada" });
        }

        return NoContent();
    }

    private static string? ValidateTask(OperationalTask task)
    {
        if (task == null)
        {
            return "Dados da tarefa são obrigatórios";
        }

        if (string.IsNullOrWhiteSpace(task.Name))
        {
            return "Nome da tarefa é obrigatório";
        }

        if (task.BatchId <= 0)
        {
            return "ID do lote é obrigatório";
        }

        if (string.IsNullOrWhiteSpace(task.TaskType))
        {
            return "Tipo de tarefa é obrigatório";
        }

        if (task.ScheduledDate == default)
        {
            return "Data agendada é obrigatória";
        }

        return null;
    }
}
