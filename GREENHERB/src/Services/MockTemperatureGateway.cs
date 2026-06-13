using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

/// <summary>
/// Mock/Stub do Gateway de Temperatura.
/// Simula leituras automáticas de sensores IoT para testes.
/// Sprint 6: Duplo para testes do gateway de temperatura automático.
/// </summary>
public class MockTemperatureGateway : ITemperatureGateway
{
    private readonly ILogger<MockTemperatureGateway> _logger;
    private readonly Random _random = new();
    private readonly Dictionary<int, List<string>> _sensorErrors = new();
    private readonly Dictionary<int, bool> _sensorAvailability = new();

    public MockTemperatureGateway(ILogger<MockTemperatureGateway> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simula a obtenção da temperatura atual com dados realistas.
    /// </summary>
    public async Task<Measurement?> GetCurrentTemperatureAsync(int batchId)
    {
        if (batchId <= 0)
        {
            _logger.LogWarning("Tentativa de obter temperatura com batchId inválido: {batchId}", batchId);
            return null;
        }

        // Verifica disponibilidade do sensor
        if (!_sensorAvailability.GetValueOrDefault(batchId, true))
        {
            _logger.LogWarning("Sensor indisponível para batch {batchId}", batchId);
            RecordSensorError(batchId, "Sensor indisponível");
            return null;
        }

        try
        {
            // Simula leitura automática com valores realistas
            var measurement = new Measurement
            {
                BatchId = batchId,
                Temperature = (decimal)(_random.NextDouble() * 40 - 10), // -10 a 30°C
                Humidity = (decimal)(_random.NextDouble() * 100),         // 0 a 100%
                Luminosity = (decimal)(_random.NextDouble() * 10000),    // 0 a 10000 lux
                MeasurementDateTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Temperatura obtida automaticamente para batch {batchId}: {temp}°C", 
                batchId, measurement.Temperature);

            return await Task.FromResult(measurement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter temperatura do gateway para batch {batchId}", batchId);
            RecordSensorError(batchId, $"Erro: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Simula múltiplas leituras automáticas em um período.
    /// </summary>
    public async Task<IEnumerable<Measurement>> GetTemperatureReadingsAsync(
        int batchId, DateTime startTime, DateTime endTime)
    {
        if (batchId <= 0)
        {
            _logger.LogWarning("Tentativa de obter leituras com batchId inválido");
            return Enumerable.Empty<Measurement>();
        }

        // Permitir start == end: deve retornar uma leitura única
        if (startTime > endTime)
        {
            _logger.LogWarning("Intervalo de tempo inválido: {start} >= {end}", startTime, endTime);
            return Enumerable.Empty<Measurement>();
        }

        try
        {
            var readings = new List<Measurement>();
            var currentTime = startTime;

            // Simula leituras em intervalos de 1 hora
            while (currentTime <= endTime)
            {
                readings.Add(new Measurement
                {
                    BatchId = batchId,
                    Temperature = (decimal)(_random.NextDouble() * 40 - 10),
                    Humidity = (decimal)(_random.NextDouble() * 100),
                    Luminosity = (decimal)(_random.NextDouble() * 10000),
                    MeasurementDateTime = currentTime,
                    CreatedAt = DateTime.UtcNow
                });

                currentTime = currentTime.AddHours(1);
            }

            _logger.LogInformation("Obtidas {count} leituras automáticas para batch {batchId}", 
                readings.Count, batchId);

            return await Task.FromResult(readings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter leituras do gateway para batch {batchId}", batchId);
            RecordSensorError(batchId, $"Erro ao obter leituras: {ex.Message}");
            return Enumerable.Empty<Measurement>();
        }
    }

    /// <summary>
    /// Simula verificação de disponibilidade do sensor.
    /// </summary>
    public async Task<bool> IsSensorAvailableAsync(int batchId)
    {
        if (batchId <= 0)
        {
            _logger.LogWarning("Verificação de sensor com batchId inválido");
            return false;
        }

        var available = _sensorAvailability.GetValueOrDefault(batchId, true);
        _logger.LogInformation("Status do sensor para batch {batchId}: {status}", 
            batchId, available ? "Disponível" : "Indisponível");

        return await Task.FromResult(available);
    }

    /// <summary>
    /// Retorna o histórico de erros do sensor.
    /// </summary>
    public async Task<IEnumerable<string>> GetSensorErrorsAsync(int batchId)
    {
        if (batchId <= 0)
        {
            return await Task.FromResult(Enumerable.Empty<string>());
        }

        var errors = _sensorErrors.GetValueOrDefault(batchId, new List<string>());
        _logger.LogInformation("Erros do sensor para batch {batchId}: {count}", batchId, errors.Count);

        return await Task.FromResult(errors);
    }

    // ===== MÉTODOS AUXILIARES PARA TESTES =====

    /// <summary>
    /// Define manualmente a disponibilidade do sensor (para testes).
    /// </summary>
    public void SetSensorAvailability(int batchId, bool isAvailable)
    {
        _sensorAvailability[batchId] = isAvailable;
        _logger.LogDebug("Disponibilidade do sensor {batchId} alterada para: {status}", 
            batchId, isAvailable);
    }

    /// <summary>
    /// Registra um erro no sensor (para testes).
    /// </summary>
    private void RecordSensorError(int batchId, string error)
    {
        if (!_sensorErrors.ContainsKey(batchId))
        {
            _sensorErrors[batchId] = new List<string>();
        }

        _sensorErrors[batchId].Add($"{DateTime.UtcNow:O} - {error}");
    }

    /// <summary>
    /// Limpa o histórico de erros (para testes).
    /// </summary>
    public void ClearSensorErrors(int batchId)
    {
        if (_sensorErrors.ContainsKey(batchId))
        {
            _sensorErrors[batchId].Clear();
            _logger.LogDebug("Erros do sensor {batchId} limpos", batchId);
        }
    }

    /// <summary>
    /// Reseta o estado do mock (para testes).
    /// </summary>
    public void Reset()
    {
        _sensorErrors.Clear();
        _sensorAvailability.Clear();
        _logger.LogInformation("MockTemperatureGateway resetado");
    }
}
