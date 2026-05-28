using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IReportService
{
    /// <summary>
    /// Obtém todos os relatórios.
    /// </summary>
    Task<IEnumerable<Report>> GetAllAsync();

    /// <summary>
    /// Obtém um relatório pelo ID.
    /// </summary>
    Task<Report?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém relatórios por tipo.
    /// </summary>
    Task<IEnumerable<Report>> GetByTypeAsync(string reportType);

    /// <summary>
    /// Cria um novo relatório.
    /// </summary>
    Task<Report> CreateAsync(Report report);

    /// <summary>
    /// Exporta um relatório em CSV.
    /// </summary>
    Task<string?> ExportToCsvAsync(int reportId);

    /// <summary>
    /// Exporta um relatório em Excel.
    /// </summary>
    Task<string?> ExportToExcelAsync(int reportId);

    /// <summary>
    /// Deleta um relatório.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
