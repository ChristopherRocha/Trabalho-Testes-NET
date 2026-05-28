using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class TaskService : ITaskService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<TaskService>? _logger;
    private bool _useMock = true;

    public TaskService(AppDbContext? context = null, ILogger<TaskService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async System.Threading.Tasks.Task<IEnumerable<OperationalTask>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todas as tarefas");
        if (_useMock)
            return MockDataProvider.GetAllTasks();

        return await _context!.OperationalTasks.ToListAsync();
    }

    public async System.Threading.Tasks.Task<OperationalTask?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo tarefa com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetTaskById(id);

        return await _context!.OperationalTasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async System.Threading.Tasks.Task<IEnumerable<OperationalTask>> GetByBatchIdAsync(int batchId)
    {
        _logger?.LogInformation("Obtendo tarefas do lote: {batchId}", batchId);
        if (_useMock)
            return MockDataProvider.GetTasksByBatchId(batchId);

        return await _context!.OperationalTasks.Where(t => t.BatchId == batchId).ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<OperationalTask>> GetByStatusAsync(string status)
    {
        _logger?.LogInformation("Obtendo tarefas com status: {status}", status);
        if (_useMock)
            return MockDataProvider.GetTasksByStatus(status);

        return await _context!.OperationalTasks.Where(t => t.Status == status).ToListAsync();
    }

    public async System.Threading.Tasks.Task<OperationalTask> CreateAsync(OperationalTask task)
    {
        _logger?.LogInformation("Criando nova tarefa: {name}", task.Name);
        if (_useMock)
        {
            var result = MockDataProvider.AddTask(task);
            _logger?.LogInformation("Tarefa criada com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.OperationalTasks.Add(task);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Tarefa criada com sucesso. ID: {id}", task.Id);
        return task;
    }

    public async System.Threading.Tasks.Task<OperationalTask?> UpdateAsync(int id, OperationalTask task)
    {
        _logger?.LogInformation("Atualizando tarefa com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.UpdateTask(id, task);
            if (result == null)
                _logger?.LogWarning("Tarefa com ID {id} não encontrada", id);
            else
                _logger?.LogInformation("Tarefa atualizada com sucesso");
            return result;
        }

        var existing = await _context!.OperationalTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null)
        {
            _logger?.LogWarning("Tarefa com ID {id} não encontrada", id);
            return null;
        }

        existing.Name = task.Name;
        existing.TaskType = task.TaskType;
        existing.Description = task.Description;
        existing.ScheduledDate = task.ScheduledDate;
        existing.CompletedDate = task.CompletedDate;
        existing.Status = task.Status;
        existing.AssignedUserId = task.AssignedUserId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger?.LogInformation("Tarefa atualizada com sucesso");
        
        return existing;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando tarefa com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteTask(id);
            if (!result)
                _logger?.LogWarning("Tarefa com ID {id} não encontrada", id);
            else
                _logger?.LogInformation("Tarefa deletada com sucesso");
            return result;
        }

        var task = await _context!.OperationalTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            _logger?.LogWarning("Tarefa com ID {id} não encontrada", id);
            return false;
        }

        _context.OperationalTasks.Remove(task);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Tarefa deletada com sucesso");
        return true;
    }
}
