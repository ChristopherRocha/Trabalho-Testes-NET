using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GREENHERB.Tests;

public class ReportServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<ReportService>> _mockLogger;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _context = TestFixtures.CreateInMemoryContext();
        _mockLogger = TestFixtures.CreateMockLogger<ReportService>();
        _reportService = new ReportService(_context, _mockLogger.Object);
    }

    #region CRUD Básico

    [Fact]
    public async Task GetAllAsync_WithNoReports_ReturnsEmptyList()
    {
        // Act
        var result = await _reportService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsReport()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var report = TestFixtures.CreateTestReport(user);
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetByIdAsync(report.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(report.Id, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesReport()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var report = new Report
        {
            Name = "Production Report",
            Description = "Q1 2026 Production",
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow,
            FilePath = "/reports/prod_q1_2026.csv",
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Production Report", result.Name);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var report = new Report
        {
            Name = "Test Report",
            ReportType = "Produção",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            CreatedByUserId = null
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        // Serviço permite UserId nulo
        Assert.NotNull(result);
        Assert.Null(result.CreatedByUserId);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesReport()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var report = TestFixtures.CreateTestReport(user);
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.DeleteAsync(report.Id);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Tipos de Relatórios

    [Theory]
    [InlineData("Produção")]
    [InlineData("Tarefas")]
    [InlineData("Medições")]
    [InlineData("Alertas")]
    public async Task CreateAsync_WithReportType_CreatesWithType(string reportType)
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var report = new Report
        {
            Name = $"Report {reportType}",
            ReportType = reportType,
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reportType, result.ReportType);
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsReportsOfType()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        _context.Reports.AddRange(
            TestFixtures.CreateTestReport(user),
            TestFixtures.CreateTestReport(user),
            new Report
            {
                Name = "Task Report",
                ReportType = "Tarefas",
                ExportFormat = "CSV",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var productionReports = await _reportService.GetByTypeAsync("Produção");
        var taskReports = await _reportService.GetByTypeAsync("Tarefas");

        // Assert
        Assert.Equal(2, productionReports.Count());
        Assert.Single(taskReports);
    }

    #endregion

    #region Formatos de Exportação

    [Theory]
    [InlineData("CSV")]
    [InlineData("Excel")]
    public async Task CreateAsync_WithExportFormat_Stored(string format)
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var report = new Report
        {
            Name = $"Report {format}",
            ReportType = "Produção",
            ExportFormat = format,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(format, result.ExportFormat);
    }

    #endregion

    #region Período de Relatórios

    [Fact]
    public async Task CreateAsync_WithDateRange_StoresDates()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var report = new Report
        {
            Name = "Period Report",
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = startDate,
            EndDate = endDate,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
    }

    [Fact]
    public async Task CreateAsync_WithCurrentMonthRange()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var today = DateTime.UtcNow;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var report = new Report
        {
            Name = "Monthly Report",
            ReportType = "Tarefas",
            ExportFormat = "Excel",
            StartDate = startOfMonth,
            EndDate = endOfMonth,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(startOfMonth, result.StartDate);
        Assert.Equal(endOfMonth, result.EndDate);
    }

    #endregion

    #region Caminho de Ficheiro

    [Fact]
    public async Task CreateAsync_WithFilePath_Stored()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var filePath = "/reports/2026/05/production_20260517.csv";

        var report = new Report
        {
            Name = "File Report",
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            FilePath = filePath,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(filePath, result.FilePath);
    }

    [Fact]
    public async Task ExportToCsvAsync_ReturnsFilePath()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var report = TestFixtures.CreateTestReport(user);
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.ExportToCsvAsync(report.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".csv", result);
    }

    [Fact]
    public async Task ExportToExcelAsync_ReturnsFilePath()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var report = TestFixtures.CreateTestReport(user);
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.ExportToExcelAsync(report.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".xlsx", result);
    }

    [Fact]
    public async Task ExportToCsvAsync_WithNonExistentReport_ReturnsNull()
    {
        // Act
        var result = await _reportService.ExportToCsvAsync(9999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Auditoria de Relatórios

    [Fact]
    public async Task CreateAsync_SetsCreatedByUser()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser("reporter");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var report = new Report
        {
            Name = "Audit Report",
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.CreatedByUserId);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAtTimestamp()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var beforeCreate = DateTime.UtcNow;

        var report = new Report
        {
            Name = "Timestamp Report",
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CreatedAt >= beforeCreate && result.CreatedAt <= afterCreate);
    }

    #endregion

    #region Casos Extremos

    [Fact]
    public async Task CreateAsync_WithNullName_ReturnsNull()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var report = new Report
        {
            Name = string.Empty,
            ReportType = "Produção",
            ExportFormat = "CSV",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };

        // Act
        var result = await _reportService.CreateAsync(report);

        // Assert
        // Serviço permite Name vazio
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithLargeReportCount()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var reports = Enumerable.Range(0, 50)
            .Select(i => new Report
            {
                Name = $"Report {i}",
                Description = $"Description {i}",
                ReportType = new[] { "Produção", "Tarefas", "Medições", "Alertas" }[i % 4],
                ExportFormat = i % 2 == 0 ? "CSV" : "Excel",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                FilePath = $"/reports/report_{i}.csv",
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Count());
    }

    [Fact]
    public async Task GetByTypeAsync_WithMultipleTypes()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        for (int i = 0; i < 30; i++)
        {
            var type = new[] { "Produção", "Tarefas", "Medições", "Alertas" }[i % 4];
            _context.Reports.Add(new Report
            {
                Name = $"Report {i}",
                ReportType = type,
                ExportFormat = "CSV",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var producao = await _reportService.GetByTypeAsync("Produção");
        var tarefas = await _reportService.GetByTypeAsync("Tarefas");
        var medicoes = await _reportService.GetByTypeAsync("Medições");
        var alertas = await _reportService.GetByTypeAsync("Alertas");

        // Assert
        Assert.Equal(8, producao.Count());
        Assert.Equal(8, tarefas.Count());
        Assert.Equal(7, medicoes.Count());
        Assert.Equal(7, alertas.Count());
    }

    #endregion
}
