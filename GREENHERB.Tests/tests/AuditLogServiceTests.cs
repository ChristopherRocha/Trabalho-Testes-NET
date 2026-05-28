using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GREENHERB.Tests;

public class AuditLogServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<AuditLogService>> _mockLogger;
    private readonly AuditLogService _auditLogService;

    public AuditLogServiceTests()
    {
        _context = TestFixtures.CreateInMemoryContext();
        _mockLogger = TestFixtures.CreateMockLogger<AuditLogService>();
        _auditLogService = new AuditLogService(_context, _mockLogger.Object);
    }

    #region CRUD Básico

    [Fact]
    public async Task GetAllAsync_WithNoAuditLogs_ReturnsEmptyList()
    {
        // Act
        var result = await _auditLogService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsAuditLog()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var log = TestFixtures.CreateTestAuditLog(user);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.GetByIdAsync(log.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(log.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _auditLogService.GetByIdAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LogOperationAsync_WithValidData_CreatesAuditLog()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.LogOperationAsync(
            user.Id,
            "Create",
            "Batch",
            Guid.NewGuid().ToString(),
            "Batch created successfully",
            null,
            "{\"name\": \"Test Batch\"}",
            "192.168.1.100"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Create", result.OperationType);
        Assert.Equal("Batch", result.EntityType);
    }



    #endregion

    #region Tipos de Operações

    [Theory]
    [InlineData("Create")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Login")]
    [InlineData("Export")]
    public async Task LogOperationAsync_WithOperationType_Stored(string operationType)
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.LogOperationAsync(
            user.Id,
            operationType,
            "Entity",
            Guid.NewGuid().ToString(),
            "Operation description",
            null,
            "{}",
            "127.0.0.1"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(operationType, result.OperationType);
    }

    [Fact]
    public async Task GetByOperationTypeAsync_ReturnsLogsOfType()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "1", Description = "Created", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Task", EntityId = "2", Description = "Created", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Update", EntityType = "Batch", EntityId = "1", Description = "Updated", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Delete", EntityType = "Task", EntityId = "2", Description = "Deleted", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var creates = await _auditLogService.GetByOperationTypeAsync("Create");
        var updates = await _auditLogService.GetByOperationTypeAsync("Update");
        var deletes = await _auditLogService.GetByOperationTypeAsync("Delete");

        // Assert
        Assert.Equal(2, creates.Count());
        Assert.Single(updates);
        Assert.Single(deletes);
    }

    #endregion

    #region Auditoria por Utilizador

    [Fact]
    public async Task GetByUserIdAsync_ReturnsLogsForUser()
    {
        // Arrange
        var user1 = TestFixtures.CreateTestUser("user1");
        var user2 = TestFixtures.CreateTestUser("user2");
        _context.Users.AddRange(user1, user2);
        
        _context.AuditLogs.AddRange(
            TestFixtures.CreateTestAuditLog(user1),
            TestFixtures.CreateTestAuditLog(user1),
            TestFixtures.CreateTestAuditLog(user2)
        );
        await _context.SaveChangesAsync();

        // Act
        var user1Logs = await _auditLogService.GetByUserIdAsync(user1.Id);
        var user2Logs = await _auditLogService.GetByUserIdAsync(user2.Id);

        // Assert
        Assert.Equal(2, user1Logs.Count());
        Assert.Single(user2Logs);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithMultipleUsers_OnlyReturnsUserLogs()
    {
        // Arrange
        var users = Enumerable.Range(0, 5)
            .Select(i => TestFixtures.CreateTestUser($"user{i}"))
            .ToList();
        _context.Users.AddRange(users);
        
        foreach (var user in users)
        {
            for (int i = 0; i < 5; i++)
            {
                _context.AuditLogs.Add(TestFixtures.CreateTestAuditLog(user));
            }
        }
        await _context.SaveChangesAsync();

        // Act
        var specificUserLogs = await _auditLogService.GetByUserIdAsync(users[0].Id);

        // Assert
        Assert.Equal(5, specificUserLogs.Count());
        Assert.All(specificUserLogs, log => Assert.Equal(users[0].Id, log.UserId));
    }

    #endregion

    #region Auditoria por Entidade

    [Fact]
    public async Task GetByEntityAsync_ReturnsLogsForEntity()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var batchId = Guid.NewGuid().ToString();
        
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = batchId, Description = "Created", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Update", EntityType = "Batch", EntityId = batchId, Description = "Updated", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Update", EntityType = "Batch", EntityId = Guid.NewGuid().ToString(), Description = "Other batch", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.GetByEntityAsync("Batch", batchId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, log => Assert.Equal(batchId, log.EntityId));
    }

    [Fact]
    public async Task GetByEntityAsync_WithDifferentEntities_OnlyReturnSpecificEntity()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "batch1", Description = "Created", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Task", EntityId = "task1", Description = "Created", CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "batch2", Description = "Created", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var batchLogs = await _auditLogService.GetByEntityAsync("Batch", "batch1");
        var taskLogs = await _auditLogService.GetByEntityAsync("Task", "task1");

        // Assert
        Assert.Single(batchLogs);
        Assert.Single(taskLogs);
    }

    #endregion

    #region Intervalo de Datas

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsLogsInRange()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var today = DateTime.UtcNow.Date;
        
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "1", Description = "Old", CreatedAt = today.AddDays(-5) },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "2", Description = "In range", CreatedAt = today.AddDays(-2) },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "3", Description = "In range", CreatedAt = today },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "4", Description = "Future", CreatedAt = today.AddDays(5) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.GetByDateRangeAsync(
            today.AddDays(-3),
            today.AddDays(1)
        );

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithNoLogsInRange_ReturnsEmptyList()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var today = DateTime.UtcNow.Date;
        
        _context.AuditLogs.Add(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "1", Description = "Old", CreatedAt = today.AddDays(-30) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.GetByDateRangeAsync(
            today.AddDays(-10),
            today.AddDays(-5)
        );

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Valores Antigos e Novos

    [Fact]
    public async Task LogOperationAsync_WithOldAndNewValues_Stored()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var oldValues = "{\"name\": \"Old Name\", \"status\": \"Ativo\"}";
        var newValues = "{\"name\": \"New Name\", \"status\": \"Encerrado\"}";

        // Act
        var result = await _auditLogService.LogOperationAsync(
            user.Id,
            "Update",
            "Batch",
            "batch123",
            "Batch updated",
            oldValues,
            newValues,
            "192.168.1.1"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(oldValues, result.OldValues);
        Assert.Equal(newValues, result.NewValues);
    }

    [Fact]
    public async Task LogOperationAsync_UpdateWithoutOldValues_OnlyNewValuesSet()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.LogOperationAsync(
            user.Id,
            "Create",
            "Batch",
            "batch456",
            "Batch created",
            null,
            "{\"name\": \"New Batch\"}",
            "192.168.1.1"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.OldValues);
        Assert.NotNull(result.NewValues);
    }

    #endregion

    #region Endereço IP

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.100")]
    [InlineData("10.0.0.1")]
    [InlineData("::1")] // IPv6 loopback
    public async Task LogOperationAsync_WithIpAddress_Stored(string ipAddress)
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.LogOperationAsync(
            user.Id,
            "Login",
            "User",
            user.Id.ToString(),
            "User logged in",
            null,
            null,
            ipAddress
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ipAddress, result.IpAddress);
    }

    #endregion

    #region Limpeza de Logs Antigos

    [Fact]
    public async Task DeleteOldLogsAsync_RemovesLogsBeforeDate()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var today = DateTime.UtcNow.Date;
        
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "1", Description = "Very old", CreatedAt = today.AddDays(-365) },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "2", Description = "Old", CreatedAt = today.AddDays(-30) },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "3", Description = "Recent", CreatedAt = today }
        );
        await _context.SaveChangesAsync();

        var beforeCount = _context.AuditLogs.Count();

        // Act
        var deletedCount = await _auditLogService.DeleteOldLogsAsync(today.AddDays(-60));

        // Assert
        Assert.Equal(2, deletedCount);
        var afterCount = _context.AuditLogs.Count();
        Assert.Equal(beforeCount - 2, afterCount);
    }

    [Fact]
    public async Task DeleteOldLogsAsync_WithNoOldLogs_DoesNotDelete()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var today = DateTime.UtcNow.Date;
        
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "1", Description = "Recent 1", CreatedAt = today },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "2", Description = "Recent 2", CreatedAt = today.AddDays(-1) }
        );
        await _context.SaveChangesAsync();

        var countBefore = _context.AuditLogs.Count();

        // Act
        var deletedCount = await _auditLogService.DeleteOldLogsAsync(today.AddDays(-30));

        // Assert
        Assert.Equal(0, deletedCount);
        var countAfter = _context.AuditLogs.Count();
        Assert.Equal(countBefore, countAfter);
    }

    #endregion

    #region Casos Extremos

    [Fact]
    public async Task GetAllAsync_WithLargeAuditLogCount()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        var logs = Enumerable.Range(0, 100)
            .Select(i => new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                User = user,
                OperationType = new[] { "Create", "Update", "Delete", "Login", "Export" }[i % 5],
                EntityType = new[] { "Batch", "Task", "Measurement", "Alert" }[i % 4],
                EntityId = $"entity_{i}",
                Description = $"Operation {i}",
                OldValues = i % 2 == 0 ? "{}" : null,
                NewValues = "{}",
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();

        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _auditLogService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Count());
    }

    [Fact]
    public async Task LogOperationAsync_WithMultipleOperations_AllStored()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var results = new List<AuditLog>();
        for (int i = 0; i < 25; i++)
        {
            var log = await _auditLogService.LogOperationAsync(
                user.Id,
                new[] { "Create", "Update", "Delete" }[i % 3],
                "Batch",
                $"batch_{i}",
                $"Operation {i}",
                null,
                $"{{\"id\": {i}}}",
                "127.0.0.1"
            );
            results.Add(log);
        }

        // Assert
        Assert.All(results, log => Assert.NotNull(log));
        Assert.Equal(25, results.Count);
    }

    [Fact]
    public async Task GetByOperationTypeAsync_CompleteAuditTrail()
    {
        // Arrange
        var user = TestFixtures.CreateTestUser();
        _context.Users.Add(user);
        
        // Simular auditoria completa de um batch
        _context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Create", EntityType = "Batch", EntityId = "batch1", Description = "Batch created", CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Update", EntityType = "Batch", EntityId = "batch1", Description = "Status changed", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { Id = Guid.NewGuid().ToString(), UserId = user.Id, User = user, OperationType = "Update", EntityType = "Batch", EntityId = "batch1", Description = "Productivity updated", CreatedAt = DateTime.UtcNow.AddHours(-1) }
        );
        await _context.SaveChangesAsync();

        // Act
        var creates = await _auditLogService.GetByOperationTypeAsync("Create");
        var updates = await _auditLogService.GetByOperationTypeAsync("Update");

        // Assert
        Assert.Single(creates);
        Assert.Equal(2, updates.Count());
    }

    #endregion
}
