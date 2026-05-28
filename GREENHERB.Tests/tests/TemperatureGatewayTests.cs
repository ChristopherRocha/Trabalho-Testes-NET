using GREENHERB.src.Services;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace GREENHERB.Tests.tests;

/// <summary>
/// Testes para o Gateway de Temperatura (MockTemperatureGateway).
/// Sprint 6: Validar que o gateway simula corretamente o comportamento automático de sensores.
/// </summary>
public class TemperatureGatewayTests
{
    private readonly MockTemperatureGateway _temperatureGateway;
    private readonly Mock<ILogger<MockTemperatureGateway>> _mockLogger;

    public TemperatureGatewayTests()
    {
        _mockLogger = new Mock<ILogger<MockTemperatureGateway>>();
        _temperatureGateway = new MockTemperatureGateway(_mockLogger.Object);
    }

    #region Testes de Obtenção de Temperatura Atual

    [Fact]
    public async Task GetCurrentTemperatureAsync_WithValidBatchId_ReturnsValidMeasurement()
    {
        // Arrange
        int batchId = 1;

        // Act
        var result = await _temperatureGateway.GetCurrentTemperatureAsync(batchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(batchId, result.BatchId);
        Assert.InRange(result.Temperature, -50m, 60m); // Dentro do range de validação
        Assert.InRange(result.Humidity, 0m, 100m);
        Assert.True(result.MeasurementDateTime.Kind == DateTimeKind.Utc);
    }

    [Fact]
    public async Task GetCurrentTemperatureAsync_WithInvalidBatchId_ReturnsNull()
    {
        // Arrange
        int invalidBatchId = -1;

        // Act
        var result = await _temperatureGateway.GetCurrentTemperatureAsync(invalidBatchId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentTemperatureAsync_WithZeroBatchId_ReturnsNull()
    {
        // Arrange
        int zeroBatchId = 0;

        // Act
        var result = await _temperatureGateway.GetCurrentTemperatureAsync(zeroBatchId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentTemperatureAsync_SensorUnavailable_ReturnsNull()
    {
        // Arrange
        int batchId = 1;
        _temperatureGateway.SetSensorAvailability(batchId, false);

        // Act
        var result = await _temperatureGateway.GetCurrentTemperatureAsync(batchId);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task GetCurrentTemperatureAsync_MultipleValidBatchIds_ReturnsValidMeasurements(int batchId)
    {
        // Act
        var result = await _temperatureGateway.GetCurrentTemperatureAsync(batchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(batchId, result.BatchId);
        Assert.NotNull(result.MeasurementDateTime);
    }

    #endregion

    #region Testes de Leituras em Período

    [Fact]
    public async Task GetTemperatureReadingsAsync_WithValidPeriod_ReturnsMultipleMeasurements()
    {
        // Arrange
        int batchId = 1;
        var startTime = DateTime.UtcNow.AddDays(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _temperatureGateway.GetTemperatureReadingsAsync(batchId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        var measurements = result.ToList();
        Assert.NotEmpty(measurements);
        Assert.All(measurements, m => Assert.Equal(batchId, m.BatchId));
        Assert.All(measurements, m => Assert.True(m.MeasurementDateTime >= startTime && m.MeasurementDateTime <= endTime));
    }

    [Fact]
    public async Task GetTemperatureReadingsAsync_WithInvalidBatchId_ReturnsEmpty()
    {
        // Arrange
        int invalidBatchId = -1;
        var startTime = DateTime.UtcNow.AddDays(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _temperatureGateway.GetTemperatureReadingsAsync(invalidBatchId, startTime, endTime);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTemperatureReadingsAsync_WithInvertedPeriod_ReturnsEmpty()
    {
        // Arrange
        int batchId = 1;
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddDays(-1); // End antes de Start

        // Act
        var result = await _temperatureGateway.GetTemperatureReadingsAsync(batchId, startTime, endTime);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTemperatureReadingsAsync_WithSamePeriod_ReturnsOneMeasurement()
    {
        // Arrange
        int batchId = 1;
        var time = DateTime.UtcNow;

        // Act
        var result = await _temperatureGateway.GetTemperatureReadingsAsync(batchId, time, time);

        // Assert
        Assert.NotNull(result);
        var measurements = result.ToList();
        Assert.Single(measurements);
        Assert.Equal(batchId, measurements[0].BatchId);
    }

    [Fact]
    public async Task GetTemperatureReadingsAsync_WithLargePeriod_ReturnsMultipleReadings()
    {
        // Arrange
        int batchId = 1;
        var startTime = DateTime.UtcNow.AddDays(-7);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _temperatureGateway.GetTemperatureReadingsAsync(batchId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        var measurements = result.ToList();
        Assert.NotEmpty(measurements);
        // Deve ter várias leituras (uma a cada hora)
        Assert.True(measurements.Count > 100, $"Esperado mais de 100 leituras, obtive {measurements.Count}");
    }

    #endregion

    #region Testes de Disponibilidade do Sensor

    [Fact]
    public async Task IsSensorAvailableAsync_DefaultState_ReturnsTrue()
    {
        // Act
        var result = await _temperatureGateway.IsSensorAvailableAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsSensorAvailableAsync_WithInvalidBatchId_ReturnsFalse()
    {
        // Act
        var result = await _temperatureGateway.IsSensorAvailableAsync(-1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsSensorAvailableAsync_AfterSensorSetToUnavailable_ReturnsFalse()
    {
        // Arrange
        int batchId = 1;
        _temperatureGateway.SetSensorAvailability(batchId, false);

        // Act
        var result = await _temperatureGateway.IsSensorAvailableAsync(batchId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsSensorAvailableAsync_AfterSensorSetToAvailable_ReturnsTrue()
    {
        // Arrange
        int batchId = 1;
        _temperatureGateway.SetSensorAvailability(batchId, true);

        // Act
        var result = await _temperatureGateway.IsSensorAvailableAsync(batchId);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Testes de Erros do Sensor

    [Fact]
    public async Task GetSensorErrorsAsync_WithInvalidBatchId_ReturnsEmpty()
    {
        // Act
        var result = await _temperatureGateway.GetSensorErrorsAsync(-1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSensorErrorsAsync_WithNoPriorErrors_ReturnsEmpty()
    {
        // Act
        var result = await _temperatureGateway.GetSensorErrorsAsync(1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSensorErrorsAsync_AfterSensorError_ReturnsErrorMessage()
    {
        // Arrange
        int batchId = 1;
        _temperatureGateway.SetSensorAvailability(batchId, false);
        await _temperatureGateway.GetCurrentTemperatureAsync(batchId);

        // Act
        var result = await _temperatureGateway.GetSensorErrorsAsync(batchId);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("indisponível", result.First().ToLower());
    }

    [Fact]
    public async Task ClearSensorErrors_RemovesErrorHistory()
    {
        // Arrange
        int batchId = 1;
        _temperatureGateway.SetSensorAvailability(batchId, false);
        await _temperatureGateway.GetCurrentTemperatureAsync(batchId);
        var errorsBefore = await _temperatureGateway.GetSensorErrorsAsync(batchId);
        Assert.NotEmpty(errorsBefore);

        // Act
        _temperatureGateway.ClearSensorErrors(batchId);

        // Assert
        var errorsAfter = await _temperatureGateway.GetSensorErrorsAsync(batchId);
        Assert.Empty(errorsAfter);
    }

    #endregion

    #region Testes de Reset

    [Fact]
    public async Task Reset_RestoresDefaultState()
    {
        // Arrange
        int batchId = 1;
        _temperatureGateway.SetSensorAvailability(batchId, false);
        _temperatureGateway.ClearSensorErrors(batchId);

        // Act
        _temperatureGateway.Reset();

        // Assert
        var isAvailable = await _temperatureGateway.IsSensorAvailableAsync(batchId);
        Assert.True(isAvailable);
    }

    #endregion

    #region Testes de Validação de Dados

    [Fact]
    public async Task GetCurrentTemperatureAsync_ReturnedDataHasCorrectStructure()
    {
        // Act
        var result = await _temperatureGateway.GetCurrentTemperatureAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.BatchId > 0);
        Assert.NotEqual(default(decimal), result.Temperature);
        Assert.NotEqual(default(decimal), result.Humidity);
        Assert.NotEqual(default(decimal), result.Luminosity);
        Assert.NotEqual(default(DateTime), result.MeasurementDateTime);
        Assert.NotEqual(default(DateTime), result.CreatedAt);
    }

    [Fact]
    public async Task GetTemperatureReadingsAsync_AllReadingsHaveConsistentBatchId()
    {
        // Arrange
        int batchId = 5;
        var startTime = DateTime.UtcNow.AddHours(-12);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _temperatureGateway.GetTemperatureReadingsAsync(batchId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, measurement => Assert.Equal(batchId, measurement.BatchId));
    }

    #endregion
}
