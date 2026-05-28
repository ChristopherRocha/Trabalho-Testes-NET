using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface ITaskService
{
    /// <summary>
    /// Obtém todas as tarefas.
    /// </summary>
    Task<IEnumerable<OperationalTask>> GetAllAsync();

    /// <summary>
    /// Obtém uma tarefa pelo ID.
    /// </summary>
    Task<OperationalTask?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém todas as tarefas de um lote.
    /// </summary>
    Task<IEnumerable<OperationalTask>> GetByBatchIdAsync(int batchId);

    /// <summary>
    /// Obtém tarefas por estado.
    /// </summary>
    Task<IEnumerable<OperationalTask>> GetByStatusAsync(string status);

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    Task<OperationalTask> CreateAsync(OperationalTask task);

    /// <summary>
    /// Atualiza uma tarefa existente.
    /// </summary>
    Task<OperationalTask?> UpdateAsync(int id, OperationalTask task);

    /// <summary>
    /// Deleta uma tarefa.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
