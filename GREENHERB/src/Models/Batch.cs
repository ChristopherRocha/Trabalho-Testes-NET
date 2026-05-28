namespace GREENHERB.src.Models;

/// <summary>
/// Representa um lote de cultivo com estado, divisões, perdas e produtividade.
/// </summary>
public class Batch
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do plano de cultivo associado
    /// </summary>
    public int CultivationPlanId { get; set; }
    
    /// <summary>
    /// Estado atual do lote (Ativo, Encerrado, Suspenso)
    /// </summary>
    public string Status { get; set; } = "Ativo";
    
    /// <summary>
    /// Número de divisões do lote
    /// </summary>
    public int NumberOfDivisions { get; set; }
    
    /// <summary>
    /// Percentual de perdas no lote (0-100)
    /// </summary>
    public decimal LossPercentage { get; set; }
    
    /// <summary>
    /// Produtividade do lote em kg
    /// </summary>
    public decimal Productivity { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
