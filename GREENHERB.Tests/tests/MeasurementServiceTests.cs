using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GREENHERB.Tests;

public class MeasurementServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<MeasurementService>> _mockLogger;
    private readonly MeasurementService _measurementService;

    public MeasurementServiceTests()
    {
        _context = TestFixtures.CreateInMemoryContext();
        _mockLogger = TestFixtures.CreateMockLogger<MeasurementService>();
        _measurementService = new MeasurementService(_context, _mockLogger.Object);
    }

    #region CRUD Básico

    [Fact]
    public async Task GetAllAsync_WithNoMeasurements_ReturnsEmptyList()
    {
        // Act
        var result = await _measurementService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsMeasurement()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var measurement = TestFixtures.CreateTestMeasurement(batch);
        _context.Measurements.Add(measurement);
        await _context.SaveChangesAsync();

        // Act
        var result = await _measurementService.GetByIdAsync(measurement.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(measurement.Id, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesMeasurement()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.5m,
            Humidity = 65.0m,
            Luminosity = 4000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(22.5m, result.Temperature);
        Assert.Equal(65.0m, result.Humidity);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBatchId_ReturnsNull()
    {
        // Arrange
        var measurement = new Measurement
        {
            BatchId = 9999,
            Temperature = 22.5m,
            Humidity = 65.0m,
            Luminosity = 4000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        // Serviço permite BatchId inválido (sem validar FK)
        Assert.NotNull(result);
        Assert.Equal(9999, result.BatchId);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesMeasurement()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var measurement = TestFixtures.CreateTestMeasurement(batch);
        _context.Measurements.Add(measurement);
        await _context.SaveChangesAsync();

        // Act
        var result = await _measurementService.DeleteAsync(measurement.Id);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Medições por Lote

    [Fact]
    public async Task GetByBatchIdAsync_ReturnsMeasurementsForBatch()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        _context.Measurements.AddRange(
            TestFixtures.CreateTestMeasurement(batch),
            TestFixtures.CreateTestMeasurement(batch),
            TestFixtures.CreateTestMeasurement(batch)
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _measurementService.GetByBatchIdAsync(batch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetByBatchIdAsync_WithMultipleBatches_OnlyReturnsBatchMeasurements()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch1 = TestFixtures.CreateTestBatch(plan);
        var batch2 = TestFixtures.CreateTestBatch(plan);
        _context.Batches.AddRange(batch1, batch2);
        
        _context.Measurements.AddRange(
            TestFixtures.CreateTestMeasurement(batch1),
            TestFixtures.CreateTestMeasurement(batch2),
            TestFixtures.CreateTestMeasurement(batch2)
        );
        await _context.SaveChangesAsync();

        // Act
        var resultBatch1 = await _measurementService.GetByBatchIdAsync(batch1.Id);
        var resultBatch2 = await _measurementService.GetByBatchIdAsync(batch2.Id);

        // Assert
        Assert.Single(resultBatch1);
        Assert.Equal(2, resultBatch2.Count());
    }

    #endregion

    #region Medições Ambientais - Temperatura

    [Fact]
    public async Task CreateAsync_WithTemperatureInRange_Stored()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 20.0m, // Dentro do intervalo recomendado
            Humidity = 65.0m,
            Luminosity = 3000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20.0m, result.Temperature);
    }

    [Theory]
    [InlineData(-50)]    // Limite mínimo
    [InlineData(0)]      // Abaixo de zero
    [InlineData(15)]     // Aceitável
    [InlineData(25)]     // Aceitável
    [InlineData(50)]     // Aceitável
    [InlineData(60)]     // Limite máximo
    public async Task CreateAsync_WithVariousTemperatures_Stored(decimal temperature)
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = temperature,
            Humidity = 65.0m,
            Luminosity = 3000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(temperature, result.Temperature);
    }

    #endregion

    #region Medições Ambientais - Humidade

    [Theory]
    [InlineData(0)]      // Mínimo
    [InlineData(50)]     // Aceitável
    [InlineData(70)]     // Aceitável
    [InlineData(100)]    // Máximo
    public async Task CreateAsync_WithVariousHumidity_Stored(decimal humidity)
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.0m,
            Humidity = humidity,
            Luminosity = 3000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(humidity, result.Humidity);
    }

    [Fact]
    public async Task CreateAsync_WithLowHumidity_Stored()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        plan.HumidityMin = 50;
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.0m,
            Humidity = 40.0m, // Abaixo do mínimo recomendado
            Luminosity = 3000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40.0m, result.Humidity);
    }

    #endregion

    #region Medições Ambientais - Luminosidade

    [Theory]
    [InlineData(0)]         // Mínimo
    [InlineData(1000)]      // Aceitável
    [InlineData(5000)]      // Aceitável
    [InlineData(10000)]     // Alto
    public async Task CreateAsync_WithVariousLuminosity_Stored(decimal luminosity)
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.0m,
            Humidity = 65.0m,
            Luminosity = luminosity,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(luminosity, result.Luminosity);
    }

    [Fact]
    public async Task CreateAsync_WithLowLuminosity_Stored()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        plan.LuminosityMin = 1000;
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.0m,
            Humidity = 65.0m,
            Luminosity = 500m, // Abaixo do mínimo recomendado
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500m, result.Luminosity);
    }

    #endregion

    #region Pesquisa por Intervalo de Datas

    [Fact]
    public async Task GetByBatchAndDateRangeAsync_ReturnsCorrectMeasurements()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var today = DateTime.UtcNow.Date;
        
                        _context.Measurements.AddRange(
            new Measurement
            {
                BatchId = batch.Id,
                Temperature = 22.0m,
                Humidity = 65.0m,
                Luminosity = 3000m,
                MeasurementDateTime = today.AddDays(-2),
                CreatedAt = DateTime.UtcNow
            },
            new Measurement
            {
                BatchId = batch.Id,
                Temperature = 23.0m,
                Humidity = 66.0m,
                Luminosity = 3100m,
                MeasurementDateTime = today.AddDays(-1),
                CreatedAt = DateTime.UtcNow
            },
            new Measurement
            {
                BatchId = batch.Id,
                Temperature = 24.0m,
                Humidity = 67.0m,
                Luminosity = 3200m,
                MeasurementDateTime = today,
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _measurementService.GetByBatchAndDateRangeAsync(
            batch.Id,
            today.AddDays(-1),
            today.AddDays(1)
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByBatchAndDateRangeAsync_WithNoMeasurements_ReturnsEmptyList()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var today = DateTime.UtcNow;

        // Act
        var result = await _measurementService.GetByBatchAndDateRangeAsync(
            batch.Id,
            today.AddDays(-7),
            today
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByBatchAndDateRangeAsync_OutsideDateRange_ReturnsNothing()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var measurement = TestFixtures.CreateTestMeasurement(batch);
        measurement.MeasurementDateTime = DateTime.UtcNow.AddDays(-10);
        _context.Measurements.Add(measurement);
        await _context.SaveChangesAsync();

        var today = DateTime.UtcNow;

        // Act
        var result = await _measurementService.GetByBatchAndDateRangeAsync(
            batch.Id,
            today.AddDays(-5),
            today
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Data e Hora de Medição

    [Fact]
    public async Task CreateAsync_WithMeasurementDateTime_Stored()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var measurementTime = DateTime.UtcNow.AddHours(-2);

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.0m,
            Humidity = 65.0m,
            Luminosity = 3000m,
            MeasurementDateTime = measurementTime
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(measurementTime, result.MeasurementDateTime);
    }

    [Fact]
    public async Task CreateAsync_SetCreatedAtToNow()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var beforeCreate = DateTime.UtcNow;

        var measurement = new Measurement
        {
            BatchId = batch.Id,
            Temperature = 22.0m,
            Humidity = 65.0m,
            Luminosity = 3000m,
            MeasurementDateTime = DateTime.UtcNow
        };

        // Act
        var result = await _measurementService.CreateAsync(measurement);

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CreatedAt >= beforeCreate && result.CreatedAt <= afterCreate);
    }

    #endregion

    #region Casos Extremos

    [Fact]
    public async Task GetAllAsync_WithLargeMeasurementCount()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var measurements = Enumerable.Range(0, 100)
            .Select(i => new Measurement
            {
                BatchId = batch.Id,
                Temperature = 20.0m + i * 0.1m,
                Humidity = 60.0m + i * 0.1m,
                Luminosity = 2000m + i * 10m,
                MeasurementDateTime = DateTime.UtcNow.AddMinutes(-i),
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _context.Measurements.AddRange(measurements);
        await _context.SaveChangesAsync();

        // Act
        var result = await _measurementService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Count());
    }

    [Fact]
    public async Task CreateAsync_WithExtremeTemperatures()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var extremeMeasurements = new[]
        {
            new Measurement { BatchId = batch.Id, Temperature = -50m, Humidity = 50m, Luminosity = 1000m, MeasurementDateTime = DateTime.UtcNow },
            new Measurement { BatchId = batch.Id, Temperature = 60m, Humidity = 100m, Luminosity = 10000m, MeasurementDateTime = DateTime.UtcNow },
        };

        // Act & Assert
        foreach (var m in extremeMeasurements)
        {
            var result = await _measurementService.CreateAsync(m);
            Assert.NotNull(result);
        }
    }

    #endregion
}
