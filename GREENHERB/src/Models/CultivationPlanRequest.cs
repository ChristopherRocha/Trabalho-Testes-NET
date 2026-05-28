namespace GREENHERB.src.Models;

public class CultivationPlanRequest
{
    public DateTime StartDate { get; set; }
    public int DurationDays { get; set; }
    public int WateringFrequencyDays { get; set; }
    public string? Notes { get; set; }
}
