namespace GREENHERB.src.Models;

/// <summary>
/// DTO para criar/atualizar lotes (flat, sem wrapper)
/// </summary>
public class BatchRequest
{
    public string Name { get; set; } = string.Empty;
    
    public int HerbId { get; set; }
    
    public int CultivationPlanId { get; set; }
    
    public DateTime PlantDate { get; set; }
    
    public DateTime ExpectedHarvestDate { get; set; }
    
    public string Status { get; set; } = "Ativo";
    
    public int NumberOfDivisions { get; set; } = 1;
    
    public decimal LossPercentage { get; set; } = 0m;
    
    public decimal Productivity { get; set; } = 0m;
}
