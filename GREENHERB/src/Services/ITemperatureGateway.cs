using GREENHERB.src.Models;

namespace GREENHERB.src.Services;

/// <summary>
/// Gateway para obtenção automática de dados de temperatura de sensores externos.
/// Abstrai a comunicação com o sistema IoT/sensor responsável pelas medições.
/// Sprint 6: Duplo (Stub) para simular dados de temperatura automáticos.
/// </summary>
public interface ITemperatureGateway
{
    /// <summary>
    /// Obtém a temperatura atual de um lote específico.
    /// Este é um processo automático que simula a leitura de um sensor.
    /// </summary>
    /// <param name="batchId">ID do lote para o qual obter a temperatura</param>
    /// <returns>Objeto de medição com dados de temperatura ou null se falhar</returns>
    Task<Measurement?> GetCurrentTemperatureAsync(int batchId);

    /// <summary>
    /// Obtém múltiplas leituras de temperatura de um período.
    /// Simula múltiplas leituras automáticas do sensor.
    /// </summary>
    /// <param name="batchId">ID do lote</param>
    /// <param name="startTime">Data/hora inicial das leituras</param>
    /// <param name="endTime">Data/hora final das leituras</param>
    /// <returns>Coleção de medições automáticas</returns>
    Task<IEnumerable<Measurement>> GetTemperatureReadingsAsync(int batchId, DateTime startTime, DateTime endTime);

    /// <summary>
    /// Verifica se o sensor está respondendo para um lote específico.
    /// </summary>
    /// <param name="batchId">ID do lote</param>
    /// <returns>True se o sensor está disponível, false caso contrário</returns>
    Task<bool> IsSensorAvailableAsync(int batchId);

    /// <summary>
    /// Obtém o histórico de erros de leitura do sensor.
    /// </summary>
    /// <param name="batchId">ID do lote</param>
    /// <returns>Lista de erros ocorridos nas últimas leituras</returns>
    Task<IEnumerable<string>> GetSensorErrorsAsync(int batchId);
}
