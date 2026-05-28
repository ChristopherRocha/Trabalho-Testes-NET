using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GREENHERB.Tests;

public class AutomationServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<AutomationService>> _mockLogger;
    private readonly AutomationService _automationService;

    public AutomationServiceTests()
    {
        _context = TestFixtures.CreateInMemoryContext();
        _mockLogger = TestFixtures.CreateMockLogger<AutomationService>();
        _automationService = new AutomationService(_context, _mockLogger.Object);
    }

    #region CRUD Básico

    [Fact]
    public async Task GetAllAsync_WithNoAutomations_ReturnsEmptyList()
    {
        // Act
        var result = await _automationService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleAutomations_ReturnsAll()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var auto1 = TestFixtures.CreateTestAutomation(batch, true);
        var auto2 = TestFixtures.CreateTestAutomation(batch, false);
        _context.Automations.AddRange(auto1, auto2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsAutomation()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.GetByIdAsync(automation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(automation.Id, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesAutomation()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var automation = new Automation
        {
            Name = "New Automation",
            Description = "Test automation",
            BatchId = batch.Id,
            IsActive = true,
            TriggerCondition = "Temperature > 28",
            Action = "Activate cooling",
            OperationMode = "Manual"
        };

        // Act
        var result = await _automationService.CreateAsync(automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Automation", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesAutomation()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        automation.Name = "Updated Name";
        automation.TriggerCondition = "Humidity < 40";

        // Act
        var result = await _automationService.UpdateAsync(automation.Id, automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Humidity < 40", result.TriggerCondition);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesAutomation()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.DeleteAsync(automation.Id);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Comutação de Modo (Manual <-> Automático)

    [Fact]
    public async Task CreateAsync_WithManualMode_SetsOperationModeCorrectly()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var automation = new Automation
        {
            Name = "Manual Mode Automation",
            BatchId = batch.Id,
            TriggerCondition = "Test",
            Action = "Test Action",
            OperationMode = "Manual"
        };

        // Act
        var result = await _automationService.CreateAsync(automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Manual", result.OperationMode);
    }

    [Fact]
    public async Task CreateAsync_WithAutomaticMode_SetsOperationModeCorrectly()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var automation = new Automation
        {
            Name = "Automatic Mode Automation",
            BatchId = batch.Id,
            TriggerCondition = "Test",
            Action = "Test Action",
            OperationMode = "Automático"
        };

        // Act
        var result = await _automationService.CreateAsync(automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Automático", result.OperationMode);
    }

    [Fact]
    public async Task UpdateAsync_ChangeFromManualToAutomatic()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch);
        automation.OperationMode = "Manual";
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        automation.OperationMode = "Automático";
        var result = await _automationService.UpdateAsync(automation.Id, automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Automático", result.OperationMode);
    }

    [Fact]
    public async Task UpdateAsync_ChangeFromAutomaticToManual()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch);
        automation.OperationMode = "Automático";
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        automation.OperationMode = "Manual";
        var result = await _automationService.UpdateAsync(automation.Id, automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Manual", result.OperationMode);
    }

    #endregion

    #region Ativação e Desativação

    [Fact]
    public async Task ActivateAsync_WithInactiveAutomation_Activates()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch, false);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.ActivateAsync(automation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_WithActiveAutomation_Deactivates()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch, true);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.DeactivateAsync(automation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_WithAlreadyActiveAutomation_RemainActive()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch, true);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.ActivateAsync(automation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_WithNonExistentAutomation_ReturnsNull()
    {
        // Act
        var result = await _automationService.DeactivateAsync(9999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Automações por Lote

    [Fact]
    public async Task GetByBatchIdAsync_ReturnAutomationsForBatch()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var auto1 = TestFixtures.CreateTestAutomation(batch, true);
        var auto2 = TestFixtures.CreateTestAutomation(batch, false);
        _context.Automations.AddRange(auto1, auto2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.GetByBatchIdAsync(batch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(batch.Id, a.BatchId));
    }

    [Fact]
    public async Task GetByBatchIdAsync_WithDifferentBatches_ReturnsOnlyForSpecificBatch()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch1 = TestFixtures.CreateTestBatch(plan);
        var batch2 = TestFixtures.CreateTestBatch(plan);
        _context.Batches.AddRange(batch1, batch2);
        
        _context.Automations.AddRange(
            TestFixtures.CreateTestAutomation(batch1),
            TestFixtures.CreateTestAutomation(batch1),
            TestFixtures.CreateTestAutomation(batch2)
        );
        await _context.SaveChangesAsync();

        // Act
        var resultBatch1 = await _automationService.GetByBatchIdAsync(batch1.Id);
        var resultBatch2 = await _automationService.GetByBatchIdAsync(batch2.Id);

        // Assert
        Assert.Equal(2, resultBatch1.Count());
        Assert.Single(resultBatch2);
    }

    #endregion

    #region Automações Ativas

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveAutomations()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        _context.Automations.AddRange(
            TestFixtures.CreateTestAutomation(batch, true),
            TestFixtures.CreateTestAutomation(batch, true),
            TestFixtures.CreateTestAutomation(batch, false),
            TestFixtures.CreateTestAutomation(batch, false)
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.GetActiveAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.True(a.IsActive));
    }

    [Fact]
    public async Task GetActiveAsync_WithNoActiveAutomations_ReturnsEmptyList()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        _context.Automations.AddRange(
            TestFixtures.CreateTestAutomation(batch, false),
            TestFixtures.CreateTestAutomation(batch, false)
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.GetActiveAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Condições de Gatilho e Ações

    [Fact]
    public async Task CreateAsync_WithTemperatureTrigger_StoresCondition()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var automation = new Automation
        {
            Name = "Temperature Control",
            BatchId = batch.Id,
            TriggerCondition = "Temperature > 28",
            Action = "Activate cooling system",
            OperationMode = "Automático",
            IsActive = true
        };

        // Act
        var result = await _automationService.CreateAsync(automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Temperature > 28", result.TriggerCondition);
        Assert.Equal("Activate cooling system", result.Action);
    }

    [Fact]
    public async Task CreateAsync_WithHumidityTrigger_StoresCondition()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var automation = new Automation
        {
            Name = "Humidity Control",
            BatchId = batch.Id,
            TriggerCondition = "Humidity < 45",
            Action = "Increase watering",
            OperationMode = "Automático"
        };

        // Act
        var result = await _automationService.CreateAsync(automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Humidity < 45", result.TriggerCondition);
    }

    [Fact]
    public async Task UpdateAsync_ChangeTriggerCondition()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch);
        automation.TriggerCondition = "Old Condition";
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act
        automation.TriggerCondition = "New Condition";
        var result = await _automationService.UpdateAsync(automation.Id, automation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Condition", result.TriggerCondition);
    }

    #endregion

    #region Casos Extremos

    [Fact]
    public async Task CreateAsync_WithNullTriggerCondition_ReturnsNull()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        var automation = new Automation
        {
            Name = "Test",
            BatchId = batch.Id,
            TriggerCondition = string.Empty,
            Action = "Action"
        };

        // Act
        var result = await _automationService.CreateAsync(automation);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBatchIdAsync_WithMultipleAutomations_LargeCount()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automations = Enumerable.Range(0, 25)
            .Select(i => new Automation
            {
                Name = $"Automation {i}",
                BatchId = batch.Id,
                TriggerCondition = $"Condition {i}",
                Action = $"Action {i}",
                OperationMode = i % 2 == 0 ? "Manual" : "Automático",
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        _context.Automations.AddRange(automations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _automationService.GetByBatchIdAsync(batch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.Count());
    }

    [Fact]
    public async Task ActivateDeactivateSequence_Works()
    {
        // Arrange
        var plan = TestFixtures.CreateTestPlan();
        _context.CultivationPlans.Add(plan);
        
        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        
        var automation = TestFixtures.CreateTestAutomation(batch, false);
        _context.Automations.Add(automation);
        await _context.SaveChangesAsync();

        // Act & Assert
        var activated = await _automationService.ActivateAsync(automation.Id);
        Assert.True(activated?.IsActive ?? false);

        var deactivated = await _automationService.DeactivateAsync(automation.Id);
        Assert.False(deactivated?.IsActive ?? true);

        var reactivated = await _automationService.ActivateAsync(automation.Id);
        Assert.True(reactivated?.IsActive ?? false);
    }

    #endregion
}
