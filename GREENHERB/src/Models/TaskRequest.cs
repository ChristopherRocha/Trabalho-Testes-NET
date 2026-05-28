namespace GREENHERB.src.Models;

/// <summary>
/// DTO para criar tarefas operacionais (flat, sem wrapper)
/// </summary>
public class TaskRequest
{
    public string Name { get; set; } = string.Empty;
    
    public int BatchId { get; set; }
    
    public string TaskType { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime ScheduledDate { get; set; }
    
    public string Status { get; set; } = "Pendente";
    
    public int? AssignedUserId { get; set; }
}
