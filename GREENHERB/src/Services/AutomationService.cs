using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class AutomationService : IAutomationService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<AutomationService>? _logger;
    private bool _useMock = true;

    public AutomationService(AppDbContext? context = null, ILogger<AutomationService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<Automation>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todas as regras de automação");
        if (_useMock)
            return MockDataProvider.GetAllAutomations();

        return await _context!.Automations.ToListAsync();
    }

    public async Task<Automation?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo regra de automação com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetAutomationById(id);

        return await _context!.Automations.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Automation>> GetByBatchIdAsync(int batchId)
    {
        _logger?.LogInformation("Obtendo regras de automação do lote: {batchId}", batchId);
        if (_useMock)
            return MockDataProvider.GetAutomationsByBatchId(batchId);

        return await _context!.Automations.Where(a => a.BatchId == batchId).ToListAsync();
    }

    public async Task<IEnumerable<Automation>> GetActiveAsync()
    {
        _logger?.LogInformation("Obtendo regras de automação ativas");
        if (_useMock)
            return MockDataProvider.GetActiveAutomations();

        return await _context!.Automations.Where(a => a.IsActive).ToListAsync();
    }

    public async Task<Automation> CreateAsync(Automation automation)
    {
        _logger?.LogInformation("Criando nova regra de automação: {name}", automation.Name);
        if (_useMock)
        {
            var result = MockDataProvider.AddAutomation(automation);
            _logger?.LogInformation("Regra de automação criada com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.Automations.Add(automation);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Regra de automação criada com sucesso. ID: {id}", automation.Id);
        return automation;
    }

    public async Task<Automation?> UpdateAsync(int id, Automation automation)
    {
        _logger?.LogInformation("Atualizando regra de automação com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.UpdateAutomation(id, automation);
            if (result == null)
                _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            else
                _logger?.LogInformation("Regra de automação atualizada com sucesso");
            return result;
        }

        var existing = await _context!.Automations.FirstOrDefaultAsync(a => a.Id == id);
        if (existing == null)
        {
            _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            return null;
        }

        existing.Name = automation.Name;
        existing.Description = automation.Description;
        existing.TriggerCondition = automation.TriggerCondition;
        existing.Action = automation.Action;
        existing.OperationMode = automation.OperationMode;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger?.LogInformation("Regra de automação atualizada com sucesso");
        
        return existing;
    }

    public async Task<Automation?> ActivateAsync(int id)
    {
        _logger?.LogInformation("Ativando regra de automação com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.ActivateAutomation(id);
            if (result == null)
                _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            return result;
        }

        var automation = await _context!.Automations.FirstOrDefaultAsync(a => a.Id == id);
        if (automation == null)
        {
            _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            return null;
        }

        automation.IsActive = true;
        automation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return automation;
    }

    public async Task<Automation?> DeactivateAsync(int id)
    {
        _logger?.LogInformation("Desativando regra de automação com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeactivateAutomation(id);
            if (result == null)
                _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            return result;
        }

        var automation = await _context!.Automations.FirstOrDefaultAsync(a => a.Id == id);
        if (automation == null)
        {
            _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            return null;
        }

        automation.IsActive = false;
        automation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return automation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando regra de automação com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteAutomation(id);
            if (!result)
                _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            else
                _logger?.LogInformation("Regra de automação deletada com sucesso");
            return result;
        }

        var automation = await _context!.Automations.FirstOrDefaultAsync(a => a.Id == id);
        if (automation == null)
        {
            _logger?.LogWarning("Regra de automação com ID {id} não encontrada", id);
            return false;
        }

        _context.Automations.Remove(automation);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Regra de automação deletada com sucesso");
        return true;
    }
}
