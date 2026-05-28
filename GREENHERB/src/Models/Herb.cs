namespace GREENHERB.src.Models;

public class Herb
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CareInstructions { get; set; }
    public int CycleDays { get; set; }

    public ICollection<CultivationPlan> CultivationPlans { get; set; } = new List<CultivationPlan>();
}
