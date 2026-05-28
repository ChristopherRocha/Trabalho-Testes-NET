using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IMeasurementService
{
    /// <summary>
    /// Obtém todas as medições.
    /// </summary>
    Task<IEnumerable<Measurement>> GetAllAsync();

    /// <summary>
    /// Obtém uma medição pelo ID.
    /// </summary>
    Task<Measurement?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém todas as medições de um lote.
    /// </summary>
    Task<IEnumerable<Measurement>> GetByBatchIdAsync(int batchId);

    /// <summary>
    /// Obtém medições de um lote em um período.
    /// </summary>
    Task<IEnumerable<Measurement>> GetByBatchAndDateRangeAsync(int batchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Cria uma nova medição.
    /// </summary>
    Task<Measurement> CreateAsync(Measurement measurement);

    /// <summary>
    /// Deleta uma medição.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
