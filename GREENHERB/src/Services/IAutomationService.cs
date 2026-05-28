using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IAutomationService
{
    /// <summary>
    /// Obtém todas as regras de automação.
    /// </summary>
    Task<IEnumerable<Automation>> GetAllAsync();

    /// <summary>
    /// Obtém uma regra de automação pelo ID.
    /// </summary>
    Task<Automation?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém regras de automação de um lote.
    /// </summary>
    Task<IEnumerable<Automation>> GetByBatchIdAsync(int batchId);

    /// <summary>
    /// Obtém regras de automação ativas.
    /// </summary>
    Task<IEnumerable<Automation>> GetActiveAsync();

    /// <summary>
    /// Cria uma nova regra de automação.
    /// </summary>
    Task<Automation> CreateAsync(Automation automation);

    /// <summary>
    /// Atualiza uma regra de automação.
    /// </summary>
    Task<Automation?> UpdateAsync(int id, Automation automation);

    /// <summary>
    /// Ativa uma regra de automação.
    /// </summary>
    Task<Automation?> ActivateAsync(int id);

    /// <summary>
    /// Desativa uma regra de automação.
    /// </summary>
    Task<Automation?> DeactivateAsync(int id);

    /// <summary>
    /// Deleta uma regra de automação.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
