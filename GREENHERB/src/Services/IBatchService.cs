using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IBatchService
{
    /// <summary>
    /// Obtém todos os lotes de cultivo.
    /// </summary>
    Task<IEnumerable<Batch>> GetAllAsync();

    /// <summary>
    /// Obtém um lote pelo ID.
    /// </summary>
    Task<Batch?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém todos os lotes de um plano de cultivo.
    /// </summary>
    Task<IEnumerable<Batch>> GetByPlanIdAsync(int planId);

    /// <summary>
    /// Cria um novo lote.
    /// </summary>
    Task<Batch> CreateAsync(Batch batch);

    /// <summary>
    /// Atualiza um lote existente.
    /// </summary>
    Task<Batch?> UpdateAsync(int id, Batch batch);

    /// <summary>
    /// Deleta um lote.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
