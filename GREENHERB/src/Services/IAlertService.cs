using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

public interface IAlertService
{
    /// <summary>
    /// Obtém todos os alertas.
    /// </summary>
    Task<IEnumerable<Alert>> GetAllAsync();

    /// <summary>
    /// Obtém um alerta pelo ID.
    /// </summary>
    Task<Alert?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém alertas por tipo.
    /// </summary>
    Task<IEnumerable<Alert>> GetByTypeAsync(string alertType);
    /// <summary>
    /// Obtém alertas por estado.
    /// </summary>
    Task<IEnumerable<Alert>> GetByStatusAsync(string status);

    /// <summary>
    /// Obtém alertas de um recurso.
    /// </summary>
    Task<IEnumerable<Alert>> GetByResourceIdAsync(int resourceId);

    /// <summary>
    /// Cria um novo alerta.
    /// </summary>
    Task<Alert?> CreateAsync(Alert alert);

    /// <summary>
    /// Resolve um alerta.
    /// </summary>
    Task<Alert?> ResolveAsync(int id, int resolvedByUserId, string resolution);

    /// <summary>
    /// Ignora um alerta.
    /// </summary>
    Task<Alert?> IgnoreAsync(int id, int ignoredByUserId, string justification);

    /// <summary>
    /// Atualiza um alerta existente.
    /// </summary>
    Task<Alert?> UpdateAsync(Alert alert);

    /// <summary>
    /// Deleta um alerta.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
