namespace GREENHERB.src.Models;

/// <summary>
/// Representa um log de auditoria de operações relevantes.
/// </summary>
public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID do utilizador que realizou a operação
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Usuário de navegação
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Tipo de operação (Create, Update, Delete, Login, Export)
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// Entidade afetada (Herb, Batch, Task, etc.)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID da entidade afetada
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da operação
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Valores anteriores (para atualizações)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Novos valores (para atualizações)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Endereço IP do utilizador
    /// </summary>
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
