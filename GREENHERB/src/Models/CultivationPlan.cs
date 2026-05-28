namespace GREENHERB.src.Models;

public class CultivationPlan
{
    public int Id { get; set; }
    public int HerbId { get; set; }
    public DateTime StartDate { get; set; }
    public int DurationDays { get; set; }
    public int WateringFrequencyDays { get; set; }
    public string? Notes { get; set; }
    public Herb? Herb { get; set; }

    // Limites ambientais
    public int? TemperatureMin { get; set; }
    public int? TemperatureMax { get; set; }
    public int? HumidityMin { get; set; }
    public int? HumidityMax { get; set; }
    public int? LuminosityMin { get; set; }
    public int? LuminosityMax { get; set; }

    // Auditoria e aprovação
    public int? ApprovedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
