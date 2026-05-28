namespace GREENHERB.src.Models;

/// <summary>
/// Representa um alerta do sistema (Informativo, Aviso, Crítico).
/// </summary>
public class Alert
{
    public int Id { get; set; }
    
    /// <summary>
    /// Título do alerta
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do alerta
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de alerta (Informativo, Aviso, Crítico)
    /// </summary>
    public string AlertType { get; set; } = "Informativo";
    
    /// <summary>
    /// ID do lote ou recurso associado
    /// </summary>
    public int? ResourceId { get; set; }
    
    /// <summary>
    /// Estado do alerta (Ativo, Resolvido, Ignorado)
    /// </summary>
    public string Status { get; set; } = "Ativo";
    
    /// <summary>
    /// Justificação para ignorar ou resolver o alerta
    /// </summary>
    public string? Resolution { get; set; }
    
    /// <summary>
    /// ID do utilizador que resolveu o alerta
    /// </summary>
    public int? ResolvedByUserId { get; set; }
    
    /// <summary>
    /// Data de resolução
    /// </summary>
    public DateTime? ResolvedDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
