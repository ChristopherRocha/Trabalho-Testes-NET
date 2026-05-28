namespace GREENHERB.src.Models;

/// <summary>
/// Representa uma medição ambiental (temperatura, humidade, luminosidade).
/// </summary>
public class Measurement
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID do lote associado
    /// </summary>
    public int BatchId { get; set; }
    
    /// <summary>
    /// Temperatura em graus Celsius
    /// </summary>
    public decimal Temperature { get; set; }
    
    /// <summary>
    /// Humidade em percentual (0-100)
    /// </summary>
    public decimal Humidity { get; set; }
    
    /// <summary>
    /// Luminosidade em lux
    /// </summary>
    public decimal Luminosity { get; set; }
    
    /// <summary>
    /// Data e hora da medição
    /// </summary>
    public DateTime MeasurementDateTime { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
