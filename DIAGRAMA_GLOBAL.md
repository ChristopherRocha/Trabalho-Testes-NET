```mermaid
classDiagram
    %% GATEWAYS & MOCKS (Sprint 6)
    class ITemperatureGateway {
        <<interface>>
        +GetCurrentTemperatureAsync(batchId int)* Task~Measurement?~
        +GetTemperatureReadingsAsync(batchId int, start DateTime, end DateTime)* Task~IEnumerable~Measurement~~
        +IsSensorAvailableAsync(batchId int)* Task~bool~
        +GetSensorErrorsAsync(batchId int)* Task~IEnumerable~string~~
    }

    class MockTemperatureGateway {
        -_logger ILogger
        -_random Random
        -_sensorErrors Dictionary
        -_sensorAvailability Dictionary
        +GetCurrentTemperatureAsync(batchId int) Task~Measurement?~
        +GetTemperatureReadingsAsync(batchId int, start DateTime, end DateTime) Task~IEnumerable~Measurement~~
        +IsSensorAvailableAsync(batchId int) Task~bool~
        +GetSensorErrorsAsync(batchId int) Task~IEnumerable~string~~
        +SetSensorAvailability(batchId int, isAvailable bool) void
        +ClearSensorErrors(batchId int) void
        +Reset() void
    }

    class INotificationGateway {
        <<interface>>
        +SendEmailNotificationAsync(email string, subject string, message string)* Task~bool~
        +SendSmsNotificationAsync(phone string, message string)* Task~bool~
        +SendPushNotificationAsync(userId int, title string, message string)* Task~bool~
        +SendBatchAlertAsync(batchId int, alertType string, message string)* Task~bool~
        +IsServiceAvailableAsync()* Task~bool~
        +GetSentNotificationsAsync(limit int)* Task~IEnumerable~NotificationLog~~
    }

    class MockNotificationGateway {
        -_logger ILogger
        -_sentNotifications List~NotificationLog~
        -_isServiceAvailable bool
        +SendEmailNotificationAsync(email string, subject string, message string) Task~bool~
        +SendSmsNotificationAsync(phone string, message string) Task~bool~
        +SendPushNotificationAsync(userId int, title string, message string) Task~bool~
        +SendBatchAlertAsync(batchId int, alertType string, message string) Task~bool~
        +IsServiceAvailableAsync() Task~bool~
        +GetSentNotificationsAsync(limit int) Task~IEnumerable~NotificationLog~~
        +SetServiceAvailability(isAvailable bool) void
        +ClearNotificationHistory() void
        +GetSuccessfulNotificationCount() int
        +GetFailedNotificationCount() int
        +Reset() void
    }

    %% SERVICE INTERFACES
    class IAlertService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~Alert~~
        +GetByIdAsync(id int)* Task~Alert?~
        +CreateAsync(alert Alert)* Task~Alert~
        +UpdateAsync(alert Alert)* Task~bool~
        +DeleteAsync(id int)* Task~bool~
    }

    class IAuditLogService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~AuditLog~~
        +GetByIdAsync(id int)* Task~AuditLog?~
        +CreateAsync(log AuditLog)* Task~AuditLog~
        +GetByUserAsync(userId int)* Task~IEnumerable~AuditLog~~
    }

    class IAuthService {
        <<interface>>
        +LoginAsync(email string, password string)* Task~AuthResponse?~
        +RegisterAsync(email string, password string, name string)* Task~bool~
        +RefreshTokenAsync(token string)* Task~AuthResponse?~
        +ValidateTokenAsync(token string)* Task~bool~
    }

    class IAutomationService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~Automation~~
        +GetByIdAsync(id int)* Task~Automation?~
        +CreateAsync(automation Automation)* Task~Automation~
        +ExecuteAutomationAsync(id int)* Task~bool~
        +CheckThresholdsAsync(batchId int)* Task~bool~
    }

    class IBatchService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~Batch~~
        +GetByIdAsync(id int)* Task~Batch?~
        +CreateAsync(batch Batch)* Task~Batch~
        +UpdateAsync(batch Batch)* Task~bool~
        +DeleteAsync(id int)* Task~bool~
    }

    class IMeasurementService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~Measurement~~
        +GetByIdAsync(id int)* Task~Measurement?~
        +CreateAsync(measurement Measurement)* Task~Measurement~
        +GetByBatchAsync(batchId int)* Task~IEnumerable~Measurement~~
    }

    class IReportService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~Report~~
        +GetByIdAsync(id int)* Task~Report?~
        +GenerateReportAsync(batchId int)* Task~Report~
        +ExportAsync(reportId int)* Task~string~
    }

    class ITaskService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~OperationalTask~~
        +GetByIdAsync(id int)* Task~OperationalTask?~
        +CreateAsync(task OperationalTask)* Task~OperationalTask~
        +CompleteAsync(id int)* Task~bool~
    }

    class IUserService {
        <<interface>>
        +GetAllAsync()* Task~IEnumerable~User~~
        +GetByIdAsync(id int)* Task~User?~
        +CreateAsync(user User)* Task~User~
        +UpdateAsync(user User)* Task~bool~
        +DeleteAsync(id int)* Task~bool~
    }

    %% SERVICE IMPLEMENTATIONS
    class AlertService {
        -_alertService IAlertService
        -_notificationGateway INotificationGateway
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~Alert~~
        +GetByIdAsync(id int) Task~Alert?~
        +CreateAsync(alert Alert) Task~Alert~
        +UpdateAsync(alert Alert) Task~bool~
        +DeleteAsync(id int) Task~bool~
    }

    class AuditLogService {
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~AuditLog~~
        +GetByIdAsync(id int) Task~AuditLog?~
        +CreateAsync(log AuditLog) Task~AuditLog~
        +GetByUserAsync(userId int) Task~IEnumerable~AuditLog~~
    }

    class AuthService {
        -_userService IUserService
        -_context AppDbContext
        -_logger ILogger
        +LoginAsync(email string, password string) Task~AuthResponse?~
        +RegisterAsync(email string, password string, name string) Task~bool~
        +RefreshTokenAsync(token string) Task~AuthResponse?~
        +ValidateTokenAsync(token string) Task~bool~
    }

    class AutomationService {
        -_automationService IAutomationService
        -_temperatureGateway ITemperatureGateway
        -_notificationGateway INotificationGateway
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~Automation~~
        +GetByIdAsync(id int) Task~Automation?~
        +CreateAsync(automation Automation) Task~Automation~
        +ExecuteAutomationAsync(id int) Task~bool~
        +CheckThresholdsAsync(batchId int) Task~bool~
    }

    class BatchService {
        -_batchService IBatchService
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~Batch~~
        +GetByIdAsync(id int) Task~Batch?~
        +CreateAsync(batch Batch) Task~Batch~
        +UpdateAsync(batch Batch) Task~bool~
        +DeleteAsync(id int) Task~bool~
    }

    class HerbService {
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~Herb~~
        +GetByIdAsync(id int) Task~Herb?~
        +CreateAsync(herb Herb) Task~Herb~
        +ImportAsync(file bytes) Task~HerbImportResult~
    }

    class MeasurementService {
        -_measurementService IMeasurementService
        -_temperatureGateway ITemperatureGateway
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~Measurement~~
        +GetByIdAsync(id int) Task~Measurement?~
        +CreateAsync(measurement Measurement) Task~Measurement~
        +GetByBatchAsync(batchId int) Task~IEnumerable~Measurement~~
    }

    class PlanService {
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~CultivationPlan~~
        +GetByIdAsync(id int) Task~CultivationPlan?~
        +CreateAsync(plan CultivationPlan) Task~CultivationPlan~
        +UpdateAsync(plan CultivationPlan) Task~bool~
    }

    class ReportService {
        -_reportService IReportService
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~Report~~
        +GetByIdAsync(id int) Task~Report?~
        +GenerateReportAsync(batchId int) Task~Report~
        +ExportAsync(reportId int) Task~string~
    }

    class TaskService {
        -_taskService ITaskService
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~OperationalTask~~
        +GetByIdAsync(id int) Task~OperationalTask?~
        +CreateAsync(task OperationalTask) Task~OperationalTask~
        +CompleteAsync(id int) Task~bool~
    }

    class UserService {
        -_userService IUserService
        -_context AppDbContext
        -_logger ILogger
        +GetAllAsync() Task~IEnumerable~User~~
        +GetByIdAsync(id int) Task~User?~
        +CreateAsync(user User) Task~User~
        +UpdateAsync(user User) Task~bool~
        +DeleteAsync(id int) Task~bool~
    }

    %% CONTROLLERS
    class AlertsController {
        -_alertService IAlertService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~Alert~~~
        +GetById(id int) Task~ActionResult~Alert~~
        +Create(alert Alert) Task~ActionResult~Alert~~
        +Update(id int, alert Alert) Task~IActionResult~
        +Delete(id int) Task~IActionResult~
    }

    class AuditController {
        -_auditService IAuditLogService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~AuditLog~~~
        +GetById(id int) Task~ActionResult~AuditLog~~
        +GetByUser(userId int) Task~ActionResult~IEnumerable~AuditLog~~~
    }

    class AuthController {
        -_authService IAuthService
        -_logger ILogger
        +Login(request AuthRequest) Task~ActionResult~AuthResponse~~
        +Register(request AuthRequest) Task~IActionResult~
        +RefreshToken(token string) Task~ActionResult~AuthResponse~~
    }

    class AutomationController {
        -_automationService IAutomationService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~Automation~~~
        +GetById(id int) Task~ActionResult~Automation~~
        +Create(automation Automation) Task~ActionResult~Automation~~
        +Execute(id int) Task~IActionResult~
    }

    class BatchesController {
        -_batchService IBatchService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~Batch~~~
        +GetById(id int) Task~ActionResult~Batch~~
        +Create(batch Batch) Task~ActionResult~Batch~~
        +Update(id int, batch Batch) Task~IActionResult~
        +Delete(id int) Task~IActionResult~
    }

    class HerbsController {
        -_context AppDbContext
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~Herb~~~
        +GetById(id int) Task~ActionResult~Herb~~
        +Create(herb Herb) Task~ActionResult~Herb~~
        +Import(file IFormFile) Task~IActionResult~
    }

    class MeasurementsController {
        -_measurementService IMeasurementService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~Measurement~~~
        +GetById(id int) Task~ActionResult~Measurement~~
        +GetByBatch(batchId int) Task~ActionResult~IEnumerable~Measurement~~~
        +Create(measurement Measurement) Task~ActionResult~Measurement~~
    }

    class PlanController {
        -_context AppDbContext
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~CultivationPlan~~~
        +GetById(id int) Task~ActionResult~CultivationPlan~~
        +Create(plan CultivationPlan) Task~ActionResult~CultivationPlan~~
        +Update(id int, plan CultivationPlan) Task~IActionResult~
    }

    class ReportsController {
        -_reportService IReportService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~Report~~~
        +GetById(id int) Task~ActionResult~Report~~
        +Generate(batchId int) Task~ActionResult~Report~~
        +Export(reportId int) Task~FileResult~
    }

    class TasksController {
        -_taskService ITaskService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~OperationalTask~~~
        +GetById(id int) Task~ActionResult~OperationalTask~~
        +Create(task OperationalTask) Task~ActionResult~OperationalTask~~
        +Complete(id int) Task~IActionResult~
    }

    class UsersController {
        -_userService IUserService
        -_logger ILogger
        +GetAll() Task~ActionResult~IEnumerable~User~~~
        +GetById(id int) Task~ActionResult~User~~
        +Create(user User) Task~ActionResult~User~~
        +Update(id int, user User) Task~IActionResult~
        +Delete(id int) Task~IActionResult~
    }

    %% MODELS
    class Alert {
        +Id int
        +Title string
        +Description string
        +AlertType string
        +ResourceId int*
        +Status string
        +CreatedAt DateTime
    }

    class AuditLog {
        +Id int
        +UserId int
        +Action string
        +EntityType string
        +EntityId int
        +Changes string
        +Timestamp DateTime
    }

    class Automation {
        +Id int
        +BatchId int
        +Name string
        +Condition string
        +Action string
        +IsActive bool
        +CreatedAt DateTime
    }

    class Batch {
        +Id int
        +Name string
        +PlanId int
        +StartDate DateTime
        +EndDate DateTime*
        +Status string
        +CreatedAt DateTime
    }

    class CultivationPlan {
        +Id int
        +Name string
        +Duration int
        +Temperature decimal
        +Humidity decimal
        +Luminosity decimal
        +CreatedAt DateTime
    }

    class Herb {
        +Id int
        +Name string
        +ScientificName string
        +Family string
        +Description string
        +OptimalTemperature decimal
        +OptimalHumidity decimal
    }

    class Measurement {
        +Id int
        +BatchId int
        +Temperature decimal
        +Humidity decimal
        +Luminosity decimal
        +MeasurementDateTime DateTime
        +CreatedAt DateTime
    }

    class OperationalTask {
        +Id int
        +BatchId int
        +Title string
        +Description string
        +AssignedTo int*
        +Status string
        +DueDate DateTime*
        +CreatedAt DateTime
    }

    class Report {
        +Id int
        +BatchId int
        +Title string
        +Content string
        +GeneratedAt DateTime
        +ExportedAt DateTime*
    }

    class User {
        +Id int
        +Email string
        +FullName string
        +PasswordHash string
        +Role string
        +IsActive bool
        +CreatedAt DateTime
    }

    class NotificationLog {
        +Id int
        +RecipientIdentifier string
        +NotificationType string
        +Subject string*
        +Message string
        +Success bool
        +ErrorMessage string*
        +SentAt DateTime
    }

    %% RELATIONSHIPS - GATEWAY IMPLEMENTATIONS
    ITemperatureGateway <|.. MockTemperatureGateway
    INotificationGateway <|.. MockNotificationGateway

    %% RELATIONSHIPS - SERVICE IMPLEMENTATIONS
    IAlertService <|.. AlertService
    IAuditLogService <|.. AuditLogService
    IAuthService <|.. AuthService
    IAutomationService <|.. AutomationService
    IBatchService <|.. BatchService
    IMeasurementService <|.. MeasurementService
    IReportService <|.. ReportService
    ITaskService <|.. TaskService
    IUserService <|.. UserService

    %% RELATIONSHIPS - CONTROLLERS USE SERVICES
    AlertsController --> IAlertService
    AuditController --> IAuditLogService
    AuthController --> IAuthService
    AutomationController --> IAutomationService
    BatchesController --> IBatchService
    MeasurementsController --> IMeasurementService
    PlanController --> AppDbContext
    ReportsController --> IReportService
    TasksController --> ITaskService
    UsersController --> IUserService

    %% RELATIONSHIPS - SERVICES USE GATEWAYS
    MeasurementService --> ITemperatureGateway
    AutomationService --> ITemperatureGateway
    AlertService --> INotificationGateway
    AutomationService --> INotificationGateway

    %% RELATIONSHIPS - SERVICES USE MODELS
    AlertService --> Alert
    AuditLogService --> AuditLog
    AutomationService --> Automation
    BatchService --> Batch
    MeasurementService --> Measurement
    ReportService --> Report
    TaskService --> OperationalTask
    UserService --> User
    MockNotificationGateway --> NotificationLog
    
    %% RELATIONSHIPS - CONTROLLERS USE MODELS
    AlertsController --> Alert
    AuditController --> AuditLog
    AutomationController --> Automation
    BatchesController --> Batch
    MeasurementsController --> Measurement
    ReportsController --> Report
    TasksController --> OperationalTask
    UsersController --> User
```
