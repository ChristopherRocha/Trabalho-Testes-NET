using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;

namespace GREENHERB.src.Services;

public class MeasurementService : IMeasurementService
{
    // Comentar a linha abaixo para usar BD real; descomente para usar mock
    private readonly AppDbContext? _context;
    private readonly ILogger<MeasurementService>? _logger;
    private bool _useMock = true;

    public MeasurementService(AppDbContext? context = null, ILogger<MeasurementService>? logger = null)
    {
        _context = context;
        _logger = logger;
        _useMock = context == null; // Usa mock se context é null
    }

    public async Task<IEnumerable<Measurement>> GetAllAsync()
    {
        _logger?.LogInformation("Obtendo todas as medições");
        if (_useMock)
            return MockDataProvider.GetAllMeasurements();

        return await _context!.Measurements.ToListAsync();
    }

    public async Task<Measurement?> GetByIdAsync(int id)
    {
        _logger?.LogInformation("Obtendo medição com ID: {id}", id);
        if (_useMock)
            return MockDataProvider.GetMeasurementById(id);

        return await _context!.Measurements.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Measurement>> GetByBatchIdAsync(int batchId)
    {
        _logger?.LogInformation("Obtendo medições do lote: {batchId}", batchId);
        if (_useMock)
            return MockDataProvider.GetMeasurementsByBatchId(batchId);

        return await _context!.Measurements.Where(m => m.BatchId == batchId).OrderByDescending(m => m.MeasurementDateTime).ToListAsync();
    }

    public async Task<IEnumerable<Measurement>> GetByBatchAndDateRangeAsync(int batchId, DateTime startDate, DateTime endDate)
    {
        _logger?.LogInformation("Obtendo medições do lote {batchId} entre {start} e {end}", batchId, startDate, endDate);
        if (_useMock)
            return MockDataProvider.GetMeasurementsByBatchAndDateRange(batchId, startDate, endDate);

        return await _context!.Measurements
            .Where(m => m.BatchId == batchId && m.MeasurementDateTime >= startDate && m.MeasurementDateTime <= endDate)
            .OrderByDescending(m => m.MeasurementDateTime)
            .ToListAsync();
    }

    public async Task<Measurement> CreateAsync(Measurement measurement)
    {
        _logger?.LogInformation("Criando nova medição para lote: {batchId}", measurement.BatchId);
        if (_useMock)
        {
            var result = MockDataProvider.AddMeasurement(measurement);
            _logger?.LogInformation("Medição criada com sucesso. ID: {id}", result.Id);
            return result;
        }

        _context!.Measurements.Add(measurement);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Medição criada com sucesso. ID: {id}", measurement.Id);
        return measurement;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger?.LogInformation("Deletando medição com ID: {id}", id);
        
        if (_useMock)
        {
            var result = MockDataProvider.DeleteMeasurement(id);
            if (!result)
                _logger?.LogWarning("Medição com ID {id} não encontrada", id);
            else
                _logger?.LogInformation("Medição deletada com sucesso");
            return result;
        }

        var measurement = await _context!.Measurements.FirstOrDefaultAsync(m => m.Id == id);
        if (measurement == null)
        {
            _logger?.LogWarning("Medição com ID {id} não encontrada", id);
            return false;
        }

        _context.Measurements.Remove(measurement);
        await _context.SaveChangesAsync();
        
        _logger?.LogInformation("Medição deletada com sucesso");
        return true;
    }
}
