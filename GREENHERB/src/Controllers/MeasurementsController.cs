using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeasurementsController : ControllerBase
{
    private readonly IMeasurementService _measurementService;
    private readonly ILogger<MeasurementsController> _logger;

    public MeasurementsController(IMeasurementService measurementService, ILogger<MeasurementsController> logger)
    {
        _measurementService = measurementService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Measurement>>> GetAll()
    {
        _logger.LogInformation("Obtendo todas as medições");
        var measurements = await _measurementService.GetAllAsync();
        return Ok(measurements);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Measurement>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter medição com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var measurement = await _measurementService.GetByIdAsync(id);
        if (measurement == null)
        {
            _logger.LogWarning("Medição com ID {id} não encontrada", id);
            return NotFound(new { message = "Medição não encontrada" });
        }

        return Ok(measurement);
    }

    [HttpGet("batch/{batchId:int}")]
    public async Task<ActionResult<IEnumerable<Measurement>>> GetByBatchId([FromRoute] int batchId)
    {
        if (batchId <= 0)
        {
            return BadRequest(new { message = "ID do lote inválido" });
        }

        var measurements = await _measurementService.GetByBatchIdAsync(batchId);
        return Ok(measurements);
    }

    [HttpGet("batch/{batchId:int}/range")]
    public async Task<ActionResult<IEnumerable<Measurement>>> GetByDateRange(
        [FromRoute] int batchId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (batchId <= 0)
        {
            return BadRequest(new { message = "ID do lote inválido" });
        }

        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Data inicial deve ser anterior à data final" });
        }

        var measurements = await _measurementService.GetByBatchAndDateRangeAsync(batchId, startDate, endDate);
        return Ok(measurements);
    }

    [HttpPost]
    public async Task<ActionResult<Measurement>> Create([FromBody] MeasurementRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Tentativa de criar medição com null");
            return BadRequest(new { message = "Dados da medição são obrigatórios" });
        }

        if (request.BatchId <= 0)
        {
            return BadRequest(new { message = "ID do lote é obrigatório" });
        }

        var measurement = new Measurement
        {
            BatchId = request.BatchId,
            Temperature = request.Temperature,
            Humidity = request.Humidity,
            Luminosity = request.Luminosity,
            MeasurementDateTime = request.MeasurementDateTime ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Criando nova medição para lote: {batchId}", measurement.BatchId);
        var created = await _measurementService.CreateAsync(measurement);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Deletando medição com ID: {id}", id);
        var deleted = await _measurementService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Medição não encontrada" });
        }

        return NoContent();
    }

    private static string? ValidateMeasurement(Measurement measurement)
    {
        if (measurement == null)
        {
            return "Dados da medição são obrigatórios";
        }

        if (measurement.BatchId <= 0)
        {
            return "ID do lote é obrigatório";
        }

        if (measurement.Temperature < -50 || measurement.Temperature > 60)
        {
            return "Temperatura deve estar entre -50°C e 60°C";
        }

        if (measurement.Humidity < 0 || measurement.Humidity > 100)
        {
            return "Humidade deve estar entre 0% e 100%";
        }

        if (measurement.Luminosity < 0)
        {
            return "Luminosidade não pode ser negativa";
        }

        return null;
    }
}
