using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class AlertService : IAlertService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<AlertService>? _logger;
    private bool _useMock = true;

    public AlertService(AppDbContext? context = null, ILogger<AlertService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<Alert>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todos os alertas");
        if (_useMock)
            return MockDataProvider.GetAllAlerts();

        return await _context!.Alerts.ToListAsync();
    }

    public async Task<Alert?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo alerta com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetAlertById(id);

        return await _context!.Alerts.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Alert>> GetByTypeAsync(string alertType)
    {
        _logger?.LogInformation("Obtendo alertas do tipo: {type}", alertType);
        if (_useMock)
            return MockDataProvider.GetAlertsByType(alertType);

        return await _context!.Alerts.Where(a => a.AlertType == alertType).ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetByStatusAsync(string status)
    {
        _logger?.LogInformation("Obtendo alertas com status: {status}", status);
        if (_useMock)
            return MockDataProvider.GetAlertsByStatus(status);

        return await _context!.Alerts.Where(a => a.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetByResourceIdAsync(int resourceId)
    {
        _logger?.LogInformation("Obtendo alertas do recurso: {resourceId}", resourceId);
        if (_useMock)
            return MockDataProvider.GetAlertsByResourceId(resourceId);

        return await _context!.Alerts.Where(a => a.ResourceId == resourceId).ToListAsync();
    }

    public async Task<Alert?> CreateAsync(Alert alert)
    {
        _logger?.LogInformation("Criando novo alerta: {title}", alert.Title);
        // Validação básica
        if (string.IsNullOrWhiteSpace(alert.Title) || string.IsNullOrWhiteSpace(alert.AlertType))
        {
            _logger?.LogWarning("Alerta inválido: título ou tipo ausente");
            return null;
        }
        
        if (_useMock)
        {
            var result = MockDataProvider.AddAlert(alert);
            if (result != null)
                _logger?.LogInformation("Alerta criado com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        _logger?.LogInformation("Alerta criado com sucesso. ID: {id}", alert.Id);
        return alert;
    }

    public async Task<Alert?> ResolveAsync(int id, int resolvedByUserId, string resolution)
    {
        _logger?.LogInformation("Resolvendo alerta com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.ResolveAlert(id, resolvedByUserId, resolution);
            if (result == null)
                _logger?.LogWarning("Alerta com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Alerta resolvido com sucesso");
            return result;
        }

        var alert = await _context!.Alerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert == null)
        {
            _logger?.LogWarning("Alerta com ID {id} não encontrado", id);
            return null;
        }
        alert.Status = "Resolvido";
        alert.Resolution = resolution;
        alert.ResolvedByUserId = resolvedByUserId;
        alert.ResolvedDate = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger?.LogInformation("Alerta resolvido com sucesso");
        return alert;
    }

    public async Task<Alert?> UpdateAsync(Alert alert)
    {
        _logger?.LogInformation("Atualizando alerta com ID: {id}", alert.Id);
        
        if (_useMock)
        {
            var result = MockDataProvider.UpdateAlert(alert.Id, alert);
            if (result == null)
                _logger?.LogWarning("Alerta com ID {id} não encontrado", alert.Id);
            else
                _logger?.LogInformation("Alerta atualizado com sucesso");
            return result;
        }

        var existing = await _context!.Alerts.FirstOrDefaultAsync(a => a.Id == alert.Id);
        if (existing == null)
        {
            _logger?.LogWarning("Alerta com ID {id} não encontrado", alert.Id);
            return null;
        }
        // Atualiza campos editáveis
        existing.Title = alert.Title;
        existing.Description = alert.Description;
        existing.AlertType = alert.AlertType;
        existing.Status = alert.Status;
        existing.ResourceId = alert.ResourceId;
        existing.Resolution = alert.Resolution;
        existing.ResolvedByUserId = alert.ResolvedByUserId;
        existing.ResolvedDate = alert.ResolvedDate;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger?.LogInformation("Alerta atualizado com sucesso");
        return existing;
    }

    public async Task<Alert?> IgnoreAsync(int id, int ignoredByUserId, string justification)
    {
        _logger?.LogInformation("Ignorando alerta com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.IgnoreAlert(id, ignoredByUserId, justification);
            if (result == null)
                _logger?.LogWarning("Alerta com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Alerta ignorado com sucesso");
            return result;
        }

        var alert = await _context!.Alerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert == null)
        {
            _logger?.LogWarning("Alerta com ID {id} não encontrado", id);
            return null;
        }
        alert.Status = "Ignorado";
        alert.Resolution = justification;
        alert.ResolvedByUserId = ignoredByUserId;
        alert.ResolvedDate = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger?.LogInformation("Alerta ignorado com sucesso");
        return alert;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando alerta com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteAlert(id);
            if (!result)
                _logger?.LogWarning("Alerta com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Alerta deletado com sucesso");
            return result;
        }

        var alert = await _context!.Alerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert == null)
        {
            _logger?.LogWarning("Alerta com ID {id} não encontrado", id);
            return false;
        }

        _context.Alerts.Remove(alert);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Alerta deletado com sucesso");
        return true;
    }
}
