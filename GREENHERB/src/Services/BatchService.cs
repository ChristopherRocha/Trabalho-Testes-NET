using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class BatchService : IBatchService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<BatchService>? _logger;
    private bool _useMock = true;

    public BatchService(AppDbContext? context = null, ILogger<BatchService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<Batch>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todos os lotes");
        if (_useMock)
            return MockDataProvider.GetAllBatches();

        return await _context!.Batches.ToListAsync();
    }

    public async Task<Batch?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo lote com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetBatchById(id);

        return await _context!.Batches.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Batch>> GetByPlanIdAsync(int planId)
    {
        _logger?.LogInformation("Obtendo lotes do plano: {planId}", planId);
        if (_useMock)
            return MockDataProvider.GetBatchesByPlanId(planId);

        return await _context!.Batches.Where(b => b.CultivationPlanId == planId).ToListAsync();
    }

    public async Task<Batch> CreateAsync(Batch batch)
    {
        _logger?.LogInformation("Criando novo lote: {name}", batch.Name);
        if (_useMock)
        {
            var result = MockDataProvider.AddBatch(batch);
            _logger?.LogInformation("Lote criado com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.Batches.Add(batch);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Lote criado com sucesso. ID: {id}", batch.Id);
        return batch;
    }

    public async Task<Batch?> UpdateAsync(int id, Batch batch)
    {
        _logger?.LogInformation("Atualizando lote com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.UpdateBatch(id, batch);
            if (result == null)
                _logger?.LogWarning("Lote com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Lote atualizado com sucesso");
            return result;
        }

        var existing = await _context!.Batches.FirstOrDefaultAsync(b => b.Id == id);
        if (existing == null)
        {
            _logger?.LogWarning("Lote com ID {id} não encontrado", id);
            return null;
        }

        existing.Name = batch.Name;
        existing.Status = batch.Status;
        existing.NumberOfDivisions = batch.NumberOfDivisions;
        existing.LossPercentage = batch.LossPercentage;
        existing.Productivity = batch.Productivity;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger?.LogInformation("Lote atualizado com sucesso");
        
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando lote com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteBatch(id);
            if (!result)
                _logger?.LogWarning("Lote com ID {id} não encontrado", id);
            else
                _logger?.LogInformation("Lote deletado com sucesso");
            return result;
        }

        var batch = await _context!.Batches.FirstOrDefaultAsync(b => b.Id == id);
        if (batch == null)
        {
            _logger?.LogWarning("Lote com ID {id} não encontrado", id);
            return false;
        }

        _context.Batches.Remove(batch);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Lote deletado com sucesso");
        return true;
    }
}
