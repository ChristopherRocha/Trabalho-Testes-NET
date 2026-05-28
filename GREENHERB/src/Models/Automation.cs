namespace GREENHERB.src.Models;

/// <summary>
/// Representa uma regra de automação e comutação entre modo Manual e Automático.
/// </summary>
public class Automation
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da regra de automação
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do lote associado
    /// </summary>
    public int BatchId { get; set; }
    
    /// <summary>
    /// Indica se a automação está ativa ou inativa
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Condição de disparo (ex: temperatura > 30)
    /// </summary>
    public string TriggerCondition { get; set; } = string.Empty;
    
    /// <summary>
    /// Ação a executar (ex: ativar rega)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Modo de operação (Manual, Automático)
    /// </summary>
    public string OperationMode { get; set; } = "Automático";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
