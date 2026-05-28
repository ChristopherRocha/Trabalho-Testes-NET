using GREENHERB.src.Models;
using System.Security.Cryptography;
using System.Text;

namespace GREENHERB.src.Services;

/// <summary>
/// Fornecedor de dados mock em memória para testes e desenvolvimento.
/// Comentar/descomentar o uso em Program.cs para alternar entre mock e BD real.
/// </summary>
public static class MockDataProvider
{
    // Dados em memória
    private static List<User> _users = new();
    private static List<Herb> _herbs = new();
    private static List<CultivationPlan> _plans = new();
    private static List<Batch> _batches = new();
    private static List<OperationalTask> _tasks = new();
    private static List<Measurement> _measurements = new();
    private static List<Alert> _alerts = new();
    private static List<Automation> _automations = new();
    private static List<Report> _reports = new();
    private static List<AuditLog> _auditLogs = new();

    private static int _userIdCounter = 1;
    private static int _herbIdCounter = 1;
    private static int _planIdCounter = 1;
    private static int _batchIdCounter = 1;
    private static int _taskIdCounter = 1;
    private static int _measurementIdCounter = 1;
    private static int _alertIdCounter = 1;
    private static int _automationIdCounter = 1;
    private static int _reportIdCounter = 1;

    static MockDataProvider()
    {
        InitializeData();
    }

    private static void InitializeData()
    {
        // Dados de Usuários
        _users = new List<User>
        {
            new User
            {
                Id = _userIdCounter++,
                Username = "admin",
                Email = "admin@greenherb.com",
                PasswordHash = HashPassword("Admin123!"),
                FullName = "Administrator",
                Role = UserRole.Administrador,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = _userIdCounter++,
                Username = "technician",
                Email = "tech@greenherb.com",
                PasswordHash = HashPassword("Tech123!"),
                FullName = "Técnico de Campo",
                Role = UserRole.Tecnico,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = _userIdCounter++,
                Username = "manager",
                Email = "manager@greenherb.com",
                PasswordHash = HashPassword("Manager123!"),
                FullName = "Gerente de Operações",
                Role = UserRole.Responsavel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Dados de Ervas
        _herbs = new List<Herb>
        {
            new Herb
            {
                Id = _herbIdCounter++,
                Name = "Basílico",
                ScientificName = "Ocimum basilicum",
                Category = "Culinária",
                Origin = "Índia",
                Notes = "Erva aromática muito popular",
                CareInstructions = "Regar regularmente, manter em local ensolarado",
                CycleDays = 45
            },
            new Herb
            {
                Id = _herbIdCounter++,
                Name = "Hortelã",
                ScientificName = "Mentha piperita",
                Category = "Medicinal",
                Origin = "Europa",
                Notes = "Refrescante e digestiva",
                CareInstructions = "Gosta de humidade, tolera sombra parcial",
                CycleDays = 60
            },
            new Herb
            {
                Id = _herbIdCounter++,
                Name = "Camomila",
                ScientificName = "Matricaria chamomilla",
                Category = "Medicinal",
                Origin = "Europa Central",
                Notes = "Calmante natural",
                CareInstructions = "Ambiente fresco, solo bem drenado",
                CycleDays = 75
            }
        };

        // Dados de Planos de Cultivo
        _plans = new List<CultivationPlan>
        {
            new CultivationPlan
            {
                Id = _planIdCounter++,
                HerbId = 1,
                StartDate = DateTime.UtcNow.AddDays(-10),
                DurationDays = 45,
                WateringFrequencyDays = 2,
                Notes = "Plantação de Basílico para colheita",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CultivationPlan
            {
                Id = _planIdCounter++,
                HerbId = 2,
                StartDate = DateTime.UtcNow.AddDays(-5),
                DurationDays = 60,
                WateringFrequencyDays = 1,
                Notes = "Cultivo de Hortelã",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Dados de Lotes
        _batches = new List<Batch>
        {
            new Batch
            {
                Id = _batchIdCounter++,
                CultivationPlanId = 1,
                Name = "Lote Basílico 001",
                Status = "Em Crescimento",
                NumberOfDivisions = 5,
                LossPercentage = 2.5m,
                Productivity = 98.5m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Batch
            {
                Id = _batchIdCounter++,
                CultivationPlanId = 2,
                Name = "Lote Hortelã 001",
                Status = "Em Crescimento",
                NumberOfDivisions = 4,
                LossPercentage = 1.2m,
                Productivity = 99.2m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Dados de Tarefas Operacionais
        _tasks = new List<OperationalTask>
        {
            new OperationalTask
            {
                Id = _taskIdCounter++,
                BatchId = 1,
                Name = "Irrigação",
                TaskType = "Manutenção",
                Description = "Regar o lote de basílico",
                ScheduledDate = DateTime.UtcNow.AddHours(2),
                Status = "Pendente",
                AssignedUserId = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new OperationalTask
            {
                Id = _taskIdCounter++,
                BatchId = 1,
                Name = "Verificação de Pragas",
                TaskType = "Inspeção",
                Description = "Verificar presença de pragas",
                ScheduledDate = DateTime.UtcNow.AddDays(1),
                Status = "Pendente",
                AssignedUserId = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Dados de Medições
        _measurements = new List<Measurement>
        {
            new Measurement
            {
                Id = _measurementIdCounter++,
                BatchId = 1,
                Temperature = 22.5m,
                Humidity = 65.2m,
                Luminosity = 800.0m,
                MeasurementDateTime = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow
            },
            new Measurement
            {
                Id = _measurementIdCounter++,
                BatchId = 1,
                Temperature = 23.0m,
                Humidity = 68.1m,
                Luminosity = 820.0m,
                MeasurementDateTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Dados de Alertas
        _alerts = new List<Alert>
        {
            new Alert
            {
                Id = _alertIdCounter++,
                Title = "Umidade Baixa",
                Description = "Humidade do solo abaixo do esperado",
                AlertType = "Umidade",
                Status = "Ativo",
                ResourceId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Alert
            {
                Id = _alertIdCounter++,
                Title = "Temperatura Elevada",
                Description = "Temperatura do ar acima do esperado",
                AlertType = "Temperatura",
                Status = "Ativo",
                ResourceId = 1,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        // Dados de Automações
        _automations = new List<Automation>
        {
            new Automation
            {
                Id = _automationIdCounter++,
                BatchId = 1,
                Name = "Riga Automática",
                Description = "Ativa irrigação quando humidade baixa",
                TriggerCondition = "SoilMoisture < 50%",
                Action = "Iniciar Sistema de Irrigação",
                OperationMode = "Automático",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Dados de Relatórios
        _reports = new List<Report>
        {
            new Report
            {
                Id = _reportIdCounter++,
                Name = "Relatório Mensal - Maio 2026",
                Description = "Análise de crescimento e produtividade",
                ReportType = "Mensal",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                ExportFormat = "CSV",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Dados de Logs de Auditoria
        _auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = 1,
                OperationType = "CREATE",
                EntityType = "Batch",
                EntityId = "1",
                Description = "Criação de novo lote",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };
    }

    // ============ USERS ============
    public static List<User> GetAllUsers() => new List<User>(_users);
    public static User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);
    public static User? GetUserByUsername(string username) => _users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
    public static User? GetUserByEmail(string email) => _users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
    public static User AddUser(User user)
    {
        user.Id = _userIdCounter++;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _users.Add(user);
        return user;
    }
    public static User? UpdateUser(int id, User updated)
    {
        var user = GetUserById(id);
        if (user == null) return null;
        user.Username = updated.Username;
        user.Email = updated.Email;
        user.FullName = updated.FullName;
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }
    public static bool DeleteUser(int id) => _users.RemoveAll(u => u.Id == id) > 0;

    // ============ HERBS ============
    public static List<Herb> GetAllHerbs() => new List<Herb>(_herbs);
    public static Herb? GetHerbById(int id) => _herbs.FirstOrDefault(h => h.Id == id);
    public static Herb AddHerb(Herb herb)
    {
        herb.Id = _herbIdCounter++;
        _herbs.Add(herb);
        return herb;
    }
    public static Herb? UpdateHerb(int id, Herb updated)
    {
        var herb = GetHerbById(id);
        if (herb == null) return null;
        herb.Name = updated.Name;
        herb.ScientificName = updated.ScientificName;
        herb.Category = updated.Category;
        herb.Origin = updated.Origin;
        herb.Notes = updated.Notes;
        herb.CareInstructions = updated.CareInstructions;
        herb.CycleDays = updated.CycleDays;
        return herb;
    }
    public static bool DeleteHerb(int id) => _herbs.RemoveAll(h => h.Id == id) > 0;

    // ============ PLANS ============
    public static List<CultivationPlan> GetAllPlans(int? herbId = null)
        => herbId.HasValue
            ? new List<CultivationPlan>(_plans.Where(p => p.HerbId == herbId))
            : new List<CultivationPlan>(_plans);
    public static CultivationPlan? GetPlanById(int id) => _plans.FirstOrDefault(p => p.Id == id);
    public static CultivationPlan AddPlan(CultivationPlan plan)
    {
        plan.Id = _planIdCounter++;
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        _plans.Add(plan);
        return plan;
    }
    public static CultivationPlan? UpdatePlan(int id, CultivationPlan updated)
    {
        var plan = GetPlanById(id);
        if (plan == null) return null;
        plan.StartDate = updated.StartDate;
        plan.DurationDays = updated.DurationDays;
        plan.WateringFrequencyDays = updated.WateringFrequencyDays;
        plan.Notes = updated.Notes;
        plan.UpdatedAt = DateTime.UtcNow;
        return plan;
    }
    public static bool DeletePlan(int id) => _plans.RemoveAll(p => p.Id == id) > 0;

    // ============ BATCHES ============
    public static List<Batch> GetAllBatches() => new List<Batch>(_batches);
    public static Batch? GetBatchById(int id) => _batches.FirstOrDefault(b => b.Id == id);
    public static List<Batch> GetBatchesByPlanId(int planId) => new List<Batch>(_batches.Where(b => b.CultivationPlanId == planId));
    public static Batch AddBatch(Batch batch)
    {
        batch.Id = _batchIdCounter++;
        batch.CreatedAt = DateTime.UtcNow;
        batch.UpdatedAt = DateTime.UtcNow;
        _batches.Add(batch);
        return batch;
    }
    public static Batch? UpdateBatch(int id, Batch updated)
    {
        var batch = GetBatchById(id);
        if (batch == null) return null;
        batch.Name = updated.Name;
        batch.Status = updated.Status;
        batch.NumberOfDivisions = updated.NumberOfDivisions;
        batch.LossPercentage = updated.LossPercentage;
        batch.Productivity = updated.Productivity;
        batch.UpdatedAt = DateTime.UtcNow;
        return batch;
    }
    public static bool DeleteBatch(int id) => _batches.RemoveAll(b => b.Id == id) > 0;

    // ============ TASKS ============
    public static List<OperationalTask> GetAllTasks() => new List<OperationalTask>(_tasks);
    public static OperationalTask? GetTaskById(int id) => _tasks.FirstOrDefault(t => t.Id == id);
    public static List<OperationalTask> GetTasksByBatchId(int batchId) => new List<OperationalTask>(_tasks.Where(t => t.BatchId == batchId));
    public static List<OperationalTask> GetTasksByStatus(string status) => new List<OperationalTask>(_tasks.Where(t => t.Status == status));
    public static OperationalTask AddTask(OperationalTask task)
    {
        task.Id = _taskIdCounter++;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _tasks.Add(task);
        return task;
    }
    public static OperationalTask? UpdateTask(int id, OperationalTask updated)
    {
        var task = GetTaskById(id);
        if (task == null) return null;
        task.Name = updated.Name;
        task.TaskType = updated.TaskType;
        task.Description = updated.Description;
        task.ScheduledDate = updated.ScheduledDate;
        task.CompletedDate = updated.CompletedDate;
        task.Status = updated.Status;
        task.AssignedUserId = updated.AssignedUserId;
        task.UpdatedAt = DateTime.UtcNow;
        return task;
    }
    public static bool DeleteTask(int id) => _tasks.RemoveAll(t => t.Id == id) > 0;

    // ============ MEASUREMENTS ============
    public static List<Measurement> GetAllMeasurements() => new List<Measurement>(_measurements);
    public static Measurement? GetMeasurementById(int id) => _measurements.FirstOrDefault(m => m.Id == id);
    public static List<Measurement> GetMeasurementsByBatchId(int batchId) 
        => new List<Measurement>(_measurements.Where(m => m.BatchId == batchId).OrderByDescending(m => m.MeasurementDateTime));
    public static List<Measurement> GetMeasurementsByBatchAndDateRange(int batchId, DateTime startDate, DateTime endDate)
        => new List<Measurement>(_measurements.Where(m => m.BatchId == batchId && m.MeasurementDateTime >= startDate && m.MeasurementDateTime <= endDate)
            .OrderByDescending(m => m.MeasurementDateTime));
    public static Measurement AddMeasurement(Measurement measurement)
    {
        measurement.Id = _measurementIdCounter++;
        measurement.CreatedAt = DateTime.UtcNow;
        _measurements.Add(measurement);
        return measurement;
    }
    public static bool DeleteMeasurement(int id) => _measurements.RemoveAll(m => m.Id == id) > 0;

    // ============ ALERTS ============
    public static List<Alert> GetAllAlerts() => new List<Alert>(_alerts);
    public static Alert? GetAlertById(int id) => _alerts.FirstOrDefault(a => a.Id == id);
    public static List<Alert> GetAlertsByType(string alertType) => new List<Alert>(_alerts.Where(a => a.AlertType == alertType));
    public static List<Alert> GetAlertsByStatus(string status) => new List<Alert>(_alerts.Where(a => a.Status == status));
    public static List<Alert> GetAlertsByResourceId(int resourceId) => new List<Alert>(_alerts.Where(a => a.ResourceId == resourceId));
    public static Alert? AddAlert(Alert alert)
    {
        if (string.IsNullOrWhiteSpace(alert.Title) || string.IsNullOrWhiteSpace(alert.AlertType))
            return null;
        alert.Id = _alertIdCounter++;
        alert.CreatedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;
        _alerts.Add(alert);
        return alert;
    }
    public static Alert? UpdateAlert(int id, Alert updated)
    {
        var alert = GetAlertById(id);
        if (alert == null) return null;
        alert.Title = updated.Title;
        alert.Description = updated.Description;
        alert.AlertType = updated.AlertType;
        alert.Status = updated.Status;
        alert.ResourceId = updated.ResourceId;
        alert.Resolution = updated.Resolution;
        alert.ResolvedByUserId = updated.ResolvedByUserId;
        alert.ResolvedDate = updated.ResolvedDate;
        alert.UpdatedAt = DateTime.UtcNow;
        return alert;
    }
    public static Alert? ResolveAlert(int id, int resolvedByUserId, string resolution)
    {
        var alert = GetAlertById(id);
        if (alert == null) return null;
        alert.Status = "Resolvido";
        alert.Resolution = resolution;
        alert.ResolvedByUserId = resolvedByUserId;
        alert.ResolvedDate = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;
        return alert;
    }
    public static Alert? IgnoreAlert(int id, int ignoredByUserId, string justification)
    {
        var alert = GetAlertById(id);
        if (alert == null) return null;
        alert.Status = "Ignorado";
        alert.Resolution = justification;
        alert.ResolvedByUserId = ignoredByUserId;
        alert.ResolvedDate = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;
        return alert;
    }
    public static bool DeleteAlert(int id) => _alerts.RemoveAll(a => a.Id == id) > 0;

    // ============ AUTOMATIONS ============
    public static List<Automation> GetAllAutomations() => new List<Automation>(_automations);
    public static Automation? GetAutomationById(int id) => _automations.FirstOrDefault(a => a.Id == id);
    public static List<Automation> GetAutomationsByBatchId(int batchId) => new List<Automation>(_automations.Where(a => a.BatchId == batchId));
    public static List<Automation> GetActiveAutomations() => new List<Automation>(_automations.Where(a => a.IsActive));
    public static Automation AddAutomation(Automation automation)
    {
        automation.Id = _automationIdCounter++;
        automation.CreatedAt = DateTime.UtcNow;
        automation.UpdatedAt = DateTime.UtcNow;
        _automations.Add(automation);
        return automation;
    }
    public static Automation? UpdateAutomation(int id, Automation updated)
    {
        var automation = GetAutomationById(id);
        if (automation == null) return null;
        automation.Name = updated.Name;
        automation.Description = updated.Description;
        automation.TriggerCondition = updated.TriggerCondition;
        automation.Action = updated.Action;
        automation.OperationMode = updated.OperationMode;
        automation.UpdatedAt = DateTime.UtcNow;
        return automation;
    }
    public static Automation? ActivateAutomation(int id)
    {
        var automation = GetAutomationById(id);
        if (automation == null) return null;
        automation.IsActive = true;
        automation.UpdatedAt = DateTime.UtcNow;
        return automation;
    }
    public static Automation? DeactivateAutomation(int id)
    {
        var automation = GetAutomationById(id);
        if (automation == null) return null;
        automation.IsActive = false;
        automation.UpdatedAt = DateTime.UtcNow;
        return automation;
    }
    public static bool DeleteAutomation(int id) => _automations.RemoveAll(a => a.Id == id) > 0;

    // ============ REPORTS ============
    public static List<Report> GetAllReports() => new List<Report>(_reports);
    public static Report? GetReportById(int id) => _reports.FirstOrDefault(r => r.Id == id);
    public static List<Report> GetReportsByType(string reportType) => new List<Report>(_reports.Where(r => r.ReportType == reportType));
    public static Report AddReport(Report report)
    {
        report.Id = _reportIdCounter++;
        report.CreatedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        _reports.Add(report);
        return report;
    }
    public static bool DeleteReport(int id) => _reports.RemoveAll(r => r.Id == id) > 0;

    // ============ AUDIT LOGS ============
    public static List<AuditLog> GetAllAuditLogs() => new List<AuditLog>(_auditLogs.OrderByDescending(a => a.CreatedAt).ToList());
    public static AuditLog? GetAuditLogById(string id) => _auditLogs.FirstOrDefault(a => a.Id == id);
    public static List<AuditLog> GetAuditLogsByUserId(int userId) => new List<AuditLog>(_auditLogs.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt));
    public static List<AuditLog> GetAuditLogsByEntity(string entityType, string entityId) 
        => new List<AuditLog>(_auditLogs.Where(a => a.EntityType == entityType && a.EntityId == entityId).OrderByDescending(a => a.CreatedAt));
    public static List<AuditLog> GetAuditLogsByOperationType(string operationType) 
        => new List<AuditLog>(_auditLogs.Where(a => a.OperationType == operationType).OrderByDescending(a => a.CreatedAt));
    public static List<AuditLog> GetAuditLogsByDateRange(DateTime startDate, DateTime endDate)
        => new List<AuditLog>(_auditLogs.Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate).OrderByDescending(a => a.CreatedAt));
    public static AuditLog LogOperation(int userId, string operationType, string entityType, string entityId, string description, string? oldValues = null, string? newValues = null, string? ipAddress = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            OperationType = operationType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        _auditLogs.Add(auditLog);
        return auditLog;
    }
    public static int DeleteOldAuditLogs(DateTime beforeDate)
    {
        var count = _auditLogs.RemoveAll(a => a.CreatedAt < beforeDate);
        return count;
    }

    // ============ HELPER ============
    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
