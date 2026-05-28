namespace GREENHERB.src.Models;

/// <summary>
/// Representa um relatório exportável em CSV ou Excel.
/// </summary>
public class Report
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do relatório
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de relatório (Produção, Tarefas, Medições, Alertas)
    /// </summary>
    public string ReportType { get; set; } = string.Empty;
    
    /// <summary>
    /// Formato de exportação (CSV, Excel)
    /// </summary>
    public string ExportFormat { get; set; } = "CSV";
    
    /// <summary>
    /// Data de início do período do relatório
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Data de fim do período do relatório
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Caminho do arquivo gerado
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// ID do utilizador que criou o relatório
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
