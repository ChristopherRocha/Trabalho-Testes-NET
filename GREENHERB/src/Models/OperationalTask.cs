namespace GREENHERB.src.Models;

/// <summary>
/// Representa uma tarefa operacional (rega, fertilização, colheita, monitorização).
/// </summary>
public class OperationalTask
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do lote associado
    /// </summary>
    public int BatchId { get; set; }
    
    /// <summary>
    /// Tipo de tarefa (Rega, Fertilização, Colheita, Monitorização)
    /// </summary>
    public string TaskType { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da tarefa
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Data prevista de execução
    /// </summary>
    public DateTime ScheduledDate { get; set; }
    
    /// <summary>
    /// Data de execução efetiva
    /// </summary>
    public DateTime? CompletedDate { get; set; }
    
    /// <summary>
    /// Estado da tarefa (Pendente, Em Andamento, Concluída, Cancelada)
    /// </summary>
    public string Status { get; set; } = "Pendente";
    
    /// <summary>
    /// Utilizador responsável pela tarefa
    /// </summary>
    public int? AssignedUserId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
