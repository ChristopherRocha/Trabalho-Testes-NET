namespace GREENHERB.src.Models;

/// <summary>
/// DTO para criar medições (flat, sem wrapper)
/// </summary>
public class MeasurementRequest
{
    public int BatchId { get; set; }
    
    public decimal Temperature { get; set; }
    
    public decimal Humidity { get; set; }
    
    public decimal Luminosity { get; set; } = 0m;
    
    public DateTime? MeasurementDateTime { get; set; }
}
