using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GREENHERB.Tests;

public class AlertServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<AlertService>> _mockLogger;
    private readonly AlertService _alertService;

    public AlertServiceTests()
    {
        _context = TestFixtures.CreateInMemoryContext();
        _mockLogger = TestFixtures.CreateMockLogger<AlertService>();
        _alertService = new AlertService(_context, _mockLogger.Object);
    }

    #region CRUD Básico

    [Fact]
    public async Task GetAllAsync_WithNoAlerts_ReturnsEmptyList()
    {
        // Act
        var result = await _alertService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleAlerts_ReturnsAll()
    {
        // Arrange
        var alert1 = TestFixtures.CreateTestAlert("Informativo", "Ativo");
        var alert2 = TestFixtures.CreateTestAlert("Aviso", "Ativo");
        _context.Alerts.AddRange(alert1, alert2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsAlert()
    {
        // Arrange
        var alert = TestFixtures.CreateTestAlert();
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.GetByIdAsync(alert.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(alert.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _alertService.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesAlert()
    {
        // Arrange
        var alert = new Alert
        {
            Title = "Temperature Alert",
            Description = "Temperature exceeds threshold",
            AlertType = "Crítico",
            ResourceId = 1,
            Status = "Ativo"
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Temperature Alert", result.Title);
        Assert.Equal("Crítico", result.AlertType);
    }

    [Fact]
    public async Task CreateAsync_WithNullTitle_ReturnsNull()
    {
        // Arrange
        var alert = new Alert
        {
            Title = null,
            Description = "No title",
            AlertType = "Informativo"
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesAlert()
    {
        // Arrange
        var alert = TestFixtures.CreateTestAlert();
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        alert.Title = "Updated Alert";

        // Act
        var result = await _alertService.UpdateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Alert", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesAlert()
    {
        // Arrange
        var alert = TestFixtures.CreateTestAlert();
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.DeleteAsync(alert.Id);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Classificação de Alertas

    [Theory]
    [InlineData("Informativo")]
    [InlineData("Aviso")]
    [InlineData("Crítico")]
    public async Task CreateAsync_WithAllAlertTypes_CreatesWithCorrectType(string alertType)
    {
        // Arrange
        var alert = new Alert
        {
            Title = $"Alert Type {alertType}",
            AlertType = alertType,
            ResourceId = 1,
            Status = "Ativo"
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(alertType, result.AlertType);
    }

    [Fact]
    public async Task GetByTypeAsync_WithValidType_ReturnsAlertsOfType()
    {
        // Arrange
        var critical1 = TestFixtures.CreateTestAlert("Crítico", "Ativo");
        var critical2 = TestFixtures.CreateTestAlert("Crítico", "Ativo");
        var warning = TestFixtures.CreateTestAlert("Aviso", "Ativo");
        _context.Alerts.AddRange(critical1, critical2, warning);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.GetByTypeAsync("Crítico");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal("Crítico", a.AlertType));
    }

    [Fact]
    public async Task GetByTypeAsync_WithNoAlertsOfType_ReturnsEmptyList()
    {
        // Arrange
        var alert = TestFixtures.CreateTestAlert("Informativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.GetByTypeAsync("Crítico");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTypeAsync_DifferentiatesBetweenTypes()
    {
        // Arrange
        _context.Alerts.AddRange(
            TestFixtures.CreateTestAlert("Informativo", "Ativo"),
            TestFixtures.CreateTestAlert("Aviso", "Ativo"),
            TestFixtures.CreateTestAlert("Crítico", "Ativo")
        );
        await _context.SaveChangesAsync();

        // Act
        var infoAlerts = await _alertService.GetByTypeAsync("Informativo");
        var warningAlerts = await _alertService.GetByTypeAsync("Aviso");
        var criticalAlerts = await _alertService.GetByTypeAsync("Crítico");

        // Assert
        Assert.Single(infoAlerts);
        Assert.Single(warningAlerts);
        Assert.Single(criticalAlerts);
    }

    #endregion

    #region Estados de Alerta

    [Theory]
    [InlineData("Ativo")]
    [InlineData("Resolvido")]
    [InlineData("Ignorado")]
    public async Task CreateAsync_WithValidStatus_CreatesWithStatus(string status)
    {
        // Arrange
        var alert = new Alert
        {
            Title = "Status Test",
            AlertType = "Aviso",
            Status = status,
            ResourceId = 1
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(status, result.Status);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsAlertsWithStatus()
    {
        // Arrange
        var active1 = TestFixtures.CreateTestAlert("Informativo", "Ativo");
        var active2 = TestFixtures.CreateTestAlert("Aviso", "Ativo");
        var resolved = TestFixtures.CreateTestAlert("Crítico", "Resolvido");
        _context.Alerts.AddRange(active1, active2, resolved);
        await _context.SaveChangesAsync();

        // Act
        var activeAlerts = await _alertService.GetByStatusAsync("Ativo");
        var resolvedAlerts = await _alertService.GetByStatusAsync("Resolvido");

        // Assert
        Assert.Equal(2, activeAlerts.Count());
        Assert.Single(resolvedAlerts);
    }

    #endregion

    #region Resolução de Alertas

    [Fact]
    public async Task ResolveAsync_WithValidUser_MarksAlertResolved()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser("resolver", UserRole.Tecnico);
        _context.Users.Add(user);
        
        var alert = TestFixtures.CreateTestAlert("Crítico", "Ativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.ResolveAsync(alert.Id, user.Id, "Problema resolvido");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resolvido", result.Status);
        Assert.Equal(user.Id, result.ResolvedByUserId);
        Assert.NotNull(result.ResolvedDate);
        Assert.Equal("Problema resolvido", result.Resolution);
    }

    [Fact]
    public async Task ResolveAsync_WithNonExistentAlert_ReturnsNull()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.ResolveAsync(1, user.Id, "Resolução");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_WithoutResolution_StillMarksResolved()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var alert = TestFixtures.CreateTestAlert("Aviso", "Ativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.ResolveAsync(alert.Id, user.Id, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resolvido", result.Status);
        Assert.Null(result.Resolution);
    }

    [Fact]
    public async Task ResolveAsync_SetResolvedDate_AsCurrentTime()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var alert = TestFixtures.CreateTestAlert("Informativo", "Ativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        var beforeResolve = DateTime.UtcNow;

        // Act
        var result = await _alertService.ResolveAsync(alert.Id, user.Id, "Done");

        var afterResolve = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result.ResolvedDate);
        Assert.True(result.ResolvedDate >= beforeResolve && result.ResolvedDate <= afterResolve);
    }

    #endregion

    #region Ignorar Alertas

    [Fact]
    public async Task IgnoreAsync_WithValidUser_MarksAlertIgnored()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser("ignorer", UserRole.Responsavel);
        _context.Users.Add(user);
        
        var alert = TestFixtures.CreateTestAlert("Aviso", "Ativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.IgnoreAsync(alert.Id, user.Id, "Justificação necessária");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ignorado", result.Status);
        Assert.Equal(user.Id, result.ResolvedByUserId);
        Assert.Equal("Justificação necessária", result.Resolution);
    }

    [Fact]
    public async Task IgnoreAsync_RequiresJustification_Stored()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var alert = TestFixtures.CreateTestAlert("Crítico", "Ativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        var justification = "Falso alarme - sensor descalibrado";

        // Act
        var result = await _alertService.IgnoreAsync(alert.Id, user.Id, justification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ignorado", result.Status);
        Assert.Equal(justification, result.Resolution);
    }

    [Fact]
    public async Task IgnoreAsync_WithNonExistentAlert_ReturnsNull()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.IgnoreAsync(1, user.Id, "Justification");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Alertas por Recurso

    [Fact]
    public async Task GetByResourceIdAsync_ReturnsAlertsForResource()
    {
        // Arrange
        int resourceId = 1;
        var alert1 = TestFixtures.CreateTestAlert();
        alert1.ResourceId = resourceId;
        var alert2 = TestFixtures.CreateTestAlert();
        alert2.ResourceId = resourceId;
        var alert3 = TestFixtures.CreateTestAlert();
        alert3.ResourceId = 2;
        _context.Alerts.AddRange(alert1, alert2, alert3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _alertService.GetByResourceIdAsync(resourceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(resourceId, a.ResourceId));
    }

    [Fact]
    public async Task GetByResourceIdAsync_WithNoAlertsForResource_ReturnsEmptyList()
    {
        // Act
        var result = await _alertService.GetByResourceIdAsync(99);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Geração Automática de Alertas baseado em Medições

    [Fact]
    public async Task CreateAlert_WhenTemperatureExceedsMax_GeneratesCriticalAlert()
    {
        // Arrange - Criar plano com limites
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);

        var plan = TestFixtures.CreateTestPlan(user);
        plan.TemperatureMax = 25; // Limite máximo 25°C
        _context.CultivationPlans.Add(plan);

        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        // Cenário: Temperatura medida 28°C (acima do limite)
        var alert = new Alert
        {
            Title = "Temperature Exceeded",
            Description = "Temperature 28°C exceeds maximum 25°C",
            AlertType = "Crítico",
            ResourceId = batch.Id,
            Status = "Ativo"
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Crítico", result.AlertType);
        Assert.Equal(batch.Id, result.ResourceId);
    }

    [Fact]
    public async Task CreateAlert_WhenHumidityBelowMin_GeneratesWarningAlert()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);

        var plan = TestFixtures.CreateTestPlan(user);
        plan.HumidityMin = 50; // Limite mínimo 50%
        _context.CultivationPlans.Add(plan);

        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        // Cenário: Humidade 45% (abaixo do limite)
        var alert = new Alert
        {
            Title = "Humidity Low",
            Description = "Humidity 45% below minimum 50%",
            AlertType = "Aviso",
            ResourceId = batch.Id,
            Status = "Ativo"
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Aviso", result.AlertType);
    }

    [Fact]
    public async Task CreateAlert_WhenLuminosityBelowThreshold_GeneratesInfoAlert()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);

        var plan = TestFixtures.CreateTestPlan(user);
        plan.LuminosityMin = 1000; // Limite mínimo 1000 lux
        _context.CultivationPlans.Add(plan);

        var batch = TestFixtures.CreateTestBatch(plan);
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        // Cenário: Luminosidade 800 lux (abaixo do limite)
        var alert = new Alert
        {
            Title = "Low Light",
            Description = "Luminosity 800 lux below minimum 1000 lux",
            AlertType = "Informativo",
            ResourceId = batch.Id,
            Status = "Ativo"
        };

        // Act
        var result = await _alertService.CreateAsync(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Informativo", result.AlertType);
    }

    #endregion

    #region Casos Extremos

    [Fact]
    public async Task ResolveAsync_OnAlreadyResolvedAlert_UpdatesResolution()
    {
        // Arrange
        var user1 = TestFixtures.CreateTestUser("resolver1");
        var user2 = TestFixtures.CreateTestUser("resolver2");
        _context.Users.AddRange(user1, user2);

        var alert = TestFixtures.CreateTestAlert("Crítico", "Ativo");
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Primeira resolução
        await _alertService.ResolveAsync(alert.Id, user1.Id, "Resolução 1");

        // Segunda resolução
        var updated = await _context.Alerts.FindAsync(alert.Id);

        // Act
        var result = await _alertService.ResolveAsync(alert.Id, user2.Id, "Resolução 2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user2.Id, result.ResolvedByUserId);
        Assert.Equal("Resolução 2", result.Resolution);
    }

    [Fact]
    public async Task GetByStatusAsync_MultipleFilters()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            _context.Alerts.Add(TestFixtures.CreateTestAlert("Informativo", "Ativo"));
        }
        for (int i = 0; i < 5; i++)
        {
            _context.Alerts.Add(TestFixtures.CreateTestAlert("Aviso", "Resolvido"));
        }
        for (int i = 0; i < 3; i++)
        {
            _context.Alerts.Add(TestFixtures.CreateTestAlert("Crítico", "Ignorado"));
        }
        await _context.SaveChangesAsync();

        // Act
        var ativo = await _alertService.GetByStatusAsync("Ativo");
        var resolvido = await _alertService.GetByStatusAsync("Resolvido");
        var ignorado = await _alertService.GetByStatusAsync("Ignorado");

        // Assert
        Assert.Equal(10, ativo.Count());
        Assert.Equal(5, resolvido.Count());
        Assert.Equal(3, ignorado.Count());
    }

    #endregion
}
