using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GREENHERB.src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Report>>> GetAll()
    {
        _logger.LogInformation("Obtendo todos os relatórios");
        var reports = await _reportService.GetAllAsync();
        return Ok(reports);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Report>> GetById([FromRoute] int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Tentativa de obter relatório com ID inválido: {id}", id);
            return BadRequest(new { message = "Id inválido" });
        }

        var report = await _reportService.GetByIdAsync(id);
        if (report == null)
        {
            _logger.LogWarning("Relatório com ID {id} não encontrado", id);
            return NotFound(new { message = "Relatório não encontrado" });
        }

        return Ok(report);
    }

    [HttpGet("type/{reportType}")]
    public async Task<ActionResult<IEnumerable<Report>>> GetByType([FromRoute] string reportType)
    {
        if (string.IsNullOrWhiteSpace(reportType))
        {
            return BadRequest(new { message = "Tipo de relatório é obrigatório" });
        }

        var reports = await _reportService.GetByTypeAsync(reportType);
        return Ok(reports);
    }

    [HttpPost]
    public async Task<ActionResult<Report>> Create([FromBody] Report report)
    {
        var validationError = ValidateReport(report);
        if (validationError != null)
        {
            _logger.LogWarning("Tentativa de criar relatório com validação falha: {error}", validationError);
            return BadRequest(new { message = validationError });
        }

        _logger.LogInformation("Criando novo relatório: {name}", report.Name);
        var created = await _reportService.CreateAsync(report);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:int}/export-csv")]
    public async Task<ActionResult> ExportToCsv([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Exportando relatório {id} para CSV", id);
        var filePath = await _reportService.ExportToCsvAsync(id);
        
        if (string.IsNullOrEmpty(filePath))
        {
            return NotFound(new { message = "Relatório não encontrado" });
        }

        return Ok(new { filePath, message = "Relatório exportado com sucesso" });
    }

    [HttpGet("{id:int}/export-excel")]
    public async Task<ActionResult> ExportToExcel([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Exportando relatório {id} para Excel", id);
        var filePath = await _reportService.ExportToExcelAsync(id);
        
        if (string.IsNullOrEmpty(filePath))
        {
            return NotFound(new { message = "Relatório não encontrado" });
        }

        return Ok(new { filePath, message = "Relatório exportado com sucesso" });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Id inválido" });
        }

        _logger.LogInformation("Deletando relatório com ID: {id}", id);
        var deleted = await _reportService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Relatório não encontrado" });
        }

        return NoContent();
    }

    private static string? ValidateReport(Report report)
    {
        if (report == null)
        {
            return "Dados do relatório são obrigatórios";
        }

        if (string.IsNullOrWhiteSpace(report.Name))
        {
            return "Nome do relatório é obrigatório";
        }

        if (string.IsNullOrWhiteSpace(report.ReportType))
        {
            return "Tipo de relatório é obrigatório";
        }

        if (report.StartDate >= report.EndDate)
        {
            return "Data inicial deve ser anterior à data final";
        }

        return null;
    }
}
