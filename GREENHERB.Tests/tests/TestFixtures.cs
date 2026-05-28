using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GREENHERB.Tests;

/// <summary>
/// Fixtures e utilitários reutilizáveis para testes
/// </summary>
public static class TestFixtures
{
    /// <summary>
    /// Cria um DbContext em memória para testes (sem dependência de SGBD)
    /// </summary>
    public static AppDbContext CreateInMemoryContext(string dbName = "GreenHerbTest")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Cria um mock de ILogger para testes
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Cria um utilizador de teste com role especificado
    /// </summary>
    public static User CreateTestUser(string username = "testuser", UserRole role = UserRole.Tecnico)
    {
        return new User
        {
            Username = username,
            Email = $"{username}@example.com",
            PasswordHash = "hashedpassword123",
            FullName = "Test User",
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria um plano de cultivo para testes
    /// </summary>
    public static CultivationPlan CreateTestPlan(User? user = null, string type = "regular")
    {
        user ??= CreateTestUser();
        
        return new CultivationPlan
        {
            TemperatureMin = 15,
            TemperatureMax = 25,
            HumidityMin = 50,
            HumidityMax = 70,
            LuminosityMin = 1000,
            LuminosityMax = 5000,
            ApprovedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria um lote para testes
    /// </summary>
    public static Batch CreateTestBatch(CultivationPlan? plan = null, string status = "Ativo")
    {
        plan ??= CreateTestPlan();

        return new Batch
        {
            Name = "Test Batch",
            CultivationPlanId = plan.Id,
            Status = status,
            NumberOfDivisions = 10,
            LossPercentage = 5.5m,
            Productivity = 85.0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria uma tarefa operacional para testes
    /// </summary>
    public static OperationalTask CreateTestTask(Batch? batch = null, User? user = null, string status = "Pendente")
    {
        batch ??= CreateTestBatch();
        user ??= CreateTestUser();

        return new OperationalTask
        {
            Name = "Test Task",
            BatchId = batch.Id,
            TaskType = "Rega",
            Description = "Test task description",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            CompletedDate = null,
            Status = status,
            AssignedUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria uma medição para testes
    /// </summary>
    public static Measurement CreateTestMeasurement(Batch? batch = null)
    {
        batch ??= CreateTestBatch();

        return new Measurement
        {
            BatchId = batch.Id,
            Temperature = 20.5m,
            Humidity = 65.0m,
            Luminosity = 3000m,
            MeasurementDateTime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria um alerta para testes
    /// </summary>
    public static Alert CreateTestAlert(string type = "Informativo", string status = "Ativo")
    {
        return new Alert
        {
            Title = "Test Alert",
            Description = "Test alert description",
            AlertType = type,
            ResourceId = 1,
            Status = status,
            Resolution = null,
            ResolvedByUserId = null,
            ResolvedDate = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria uma automação para testes
    /// </summary>
    public static Automation CreateTestAutomation(Batch? batch = null, bool isActive = true)
    {
        batch ??= CreateTestBatch();

        return new Automation
        {
            Name = "Test Automation",
            Description = "Test automation rule",
            BatchId = batch.Id,
            IsActive = isActive,
            TriggerCondition = "Temperature > 25",
            Action = "Activate cooling system",
            OperationMode = "Manual",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria um relatório para testes
    /// </summary>
    public static Report CreateTestReport(User? user = null)
    {
        user ??= CreateTestUser();

        return new Report
        {
            Name = "Test Report",
            Description = "Test report",
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            FilePath = "/reports/test_report.csv",
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Cria um log de auditoria para testes
    /// </summary>
    public static AuditLog CreateTestAuditLog(User? user = null)
    {
        user ??= CreateTestUser();

        return new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            User = user,
            OperationType = "Create",
            EntityType = "Batch",
            EntityId = Guid.NewGuid().ToString(),
            Description = "Batch created",
            OldValues = null,
            NewValues = "{\"name\": \"Test Batch\"}",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Seed inicial com dados para testes
    /// </summary>
    public static AppDbContext SeedTestData(AppDbContext context)
    {
        var adminUser = CreateTestUser("admin", UserRole.Administrador);
        var techUser = CreateTestUser("tech", UserRole.Tecnico);
        var respUser = CreateTestUser("responsible", UserRole.Responsavel);

        context.Users.AddRange(adminUser, techUser, respUser);
        context.SaveChanges();

        var plan = CreateTestPlan(techUser, "regular");
        context.CultivationPlans.Add(plan);
        context.SaveChanges();

        var batch = CreateTestBatch(plan, "Ativo");
        context.Batches.Add(batch);
        context.SaveChanges();

        var task = CreateTestTask(batch, techUser, "Pendente");
        context.OperationalTasks.Add(task);
        context.SaveChanges();

        return context;
    }
}
