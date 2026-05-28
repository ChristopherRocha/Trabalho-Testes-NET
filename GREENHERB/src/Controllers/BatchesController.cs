using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;
    private readonly ILogger<BatchesController> _logger;

    public BatchesController(IBatchService batchService, ILogger<BatchesController> logger)
    {
        _batchService = batchService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Batch>>> GetAll()
    {
        _logger.LogInformation("Obtendo todos os lotes");
        var batches = await _batchService.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Batch>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter lote com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var batch = await _batchService.GetByIdAsync(id);
        if (batch == null)
        {
            _logger.LogWarning("Lote com ID {id} não encontrado", id);
            return NotFound(new { message = "Lote não encontrado" });
        }

        return Ok(batch);
    }

    [HttpGet("{id:int}/plan")]
    public async Task<ActionResult<CultivationPlan>> GetPlanByBatchId([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        var batch = await _batchService.GetByIdAsync(id);
        if (batch == null)
        {
            return NotFound(new { message = "Lote não encontrado" });
        }

        // Retornar plano mock baseado no CultivationPlanId do lote
        var plan = new CultivationPlan
        {
            Id = batch.CultivationPlanId,
            HerbId = 1,
            StartDate = DateTime.Now.AddDays(-30),
            DurationDays = 90,
            WateringFrequencyDays = 2,
            Notes = $"Plano do lote {batch.Name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(plan);
    }

    [HttpGet("plan/{planId:int}")]
    public async Task<ActionResult<IEnumerable<Batch>>> GetByPlanId([FromRoute] int planId)
    {
        if (planId <= 0)
        {
            return BadRequest(new { message = "ID do plano inválido" });
        }

        var batches = await _batchService.GetByPlanIdAsync(planId);
        return Ok(batches);
    }

    [HttpPost]
    public async Task<ActionResult<Batch>> Create([FromBody] BatchRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Tentativa de criar lote com null");
            return BadRequest(new { message = "Dados do lote são obrigatórios" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Nome do lote é obrigatório" });
        }

        if (request.CultivationPlanId <= 0)
        {
            return BadRequest(new { message = "ID do plano de cultivo é obrigatório" });
        }

        var batch = new Batch
        {
            Name = request.Name,
            CultivationPlanId = request.CultivationPlanId,
            Status = request.Status,
            NumberOfDivisions = request.NumberOfDivisions,
            LossPercentage = request.LossPercentage,
            Productivity = request.Productivity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Criando novo lote: {name}", batch.Name);
        var created = await _batchService.CreateAsync(batch);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Batch>> Update([FromRoute] int id, [FromBody] Batch updated)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        var validationError = ValidateBatch(updated);
        if (validationError != null)
        {
            _logger.LogWarning("Tentativa de atualizar lote com validação falha: {error}", validationError);
            return BadRequest(new { message = validationError });
        }

        _logger.LogInformation("Atualizando lote com ID: {id}", id);
        var updatedBatch = await _batchService.UpdateAsync(id, updated);
        if (updatedBatch == null)
        {
            return NotFound(new { message = "Lote não encontrado" });
        }

        return Ok(updatedBatch);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Deletando lote com ID: {id}", id);
        var deleted = await _batchService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Lote não encontrado" });
        }

        return NoContent();
    }

    private static string? ValidateBatch(Batch batch)
    {
        if (batch == null)
        {
            return "Dados do lote são obrigatórios";
        }

        if (string.IsNullOrWhiteSpace(batch.Name))
        {
            return "Nome do lote é obrigatório";
        }

        if (batch.CultivationPlanId <= 0)
        {
            return "ID do plano de cultivo é obrigatório";
        }

        if (batch.NumberOfDivisions < 0)
        {
            return "Número de divisões não pode ser negativo";
        }

        if (batch.LossPercentage < 0 || batch.LossPercentage > 100)
        {
            return "Percentual de perdas deve estar entre 0 e 100";
        }

        return null;
    }
}
