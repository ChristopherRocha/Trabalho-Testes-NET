using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class ReportService : IReportService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<ReportService>? _logger;
    private bool _useMock = true;

    public ReportService(AppDbContext? context = null, ILogger<ReportService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<Report>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todos os relatórios");
        if (_useMock)
            return MockDataProvider.GetAllReports();

        return await _context!.Reports.ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo relatório com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetReportById(id);

        return await _context!.Reports.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Report>> GetByTypeAsync(string reportType)
    {
        _logger?.LogInformation("Obtendo relatórios do tipo: {type}", reportType);
        if (_useMock)
            return MockDataProvider.GetReportsByType(reportType);

        return await _context!.Reports.Where(r => r.ReportType == reportType).ToListAsync();
    }

    public async Task<Report> CreateAsync(Report report)
    {
        _logger?.LogInformation("Criando novo relatório: {name}", report.Name);
        if (_useMock)
        {
            var result = MockDataProvider.AddReport(report);
            _logger?.LogInformation("Relatório criado com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.Reports.Add(report);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Relatório criado com sucesso. ID: {id}", report.Id);
        return report;
    }

    public async Task<string?> ExportToCsvAsync(int reportId)
    {
        _logger?.LogInformation("Exportando relatório {reportId} para CSV", reportId);
        
        Report? report;
        if (_useMock)
            report = MockDataProvider.GetReportById(reportId);
        else
            report = await _context!.Reports.FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
        {
            _logger?.LogWarning("Relatório com ID {reportId} não encontrado", reportId);
            return null;
        }

        // Implementação simplificada - gera caminho de arquivo
        var fileName = $"{report.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        
        _logger?.LogInformation("Relatório exportado para CSV: {path}", filePath);
        return filePath;
    }

    public async Task<string?> ExportToExcelAsync(int reportId)
    {
        _logger?.LogInformation("Exportando relatório {reportId} para Excel", reportId);
        
        Report? report;
        if (_useMock)
            report = MockDataProvider.GetReportById(reportId);
        else
            report = await _context!.Reports.FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
        {
            _logger?.LogWarning("Relatório com ID {reportId} não encontrado", reportId);
            return null;
        }

        // Implementação simplificada - gera caminho de arquivo
        var fileName = $"{report.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        
        _logger?.LogInformation("Relatório exportado para Excel: {path}", filePath);
        return filePath;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando relatório com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteReport(id);
            if (!result)
                _logger?.LogWarning("Relatório com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Relatório deletado com sucesso");
            return result;
        }

        var report = await _context!.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (report == null)
        {
            _logger?.LogWarning("Relatório com ID {id} não encontrado", id);
            return false;
        }

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Relatório deletado com sucesso");
        return true;
    }
}
