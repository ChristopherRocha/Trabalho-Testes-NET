using GREENHERB.src.Services;
using GREENHERB.src.Data.Contexts;
using GREENHERB.src.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
builder.Services.AddControllers();

// ============ USAR MOCK: Descomente as linhas abaixo para usar dados em memória ============
// IMPORTANTE: Para usar mock, mantenha as linhas abaixo comentadas e descomente a seção MOCK
// Para voltar ao BD real, comente a seção MOCK e descomente as linhas do BD abaixo

// ========== CONFIGURAÇÃO BD REAL (comentado para usar mock) ==========
/*
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var dbContext = provider.GetRequiredService<AppDbContext>();
    return new AuthService(config, dbContext);
});

builder.Services.AddScoped<HerbService>();
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserService, UserService>();
*/

// ========== CONFIGURAÇÃO MOCK (descomente para usar mock) ==========
// Services com mock (sem DbContext)
builder.Services.AddScoped<IAuthService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    
    // Criar dicionário mock com usuário testuser
    var mockUsers = new Dictionary<string, User>();
    
    // Adicionar usuário de teste: testuser / testpass
    var testUser = new User
    {
        Id = 1,
        Username = "testuser",
        Email = "testuser@greenherb.local",
        PasswordHash = new AuthService(config, null).GetType().GetMethod("HashPassword", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(new AuthService(config, null), new object[] { "testpass" }) as string ?? "",
        Role = UserRole.Tecnico,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
    
    // Gerar hash de senha (SHA256)
    using (var sha256 = System.Security.Cryptography.SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("testpass"));
        testUser.PasswordHash = Convert.ToBase64String(hashedBytes);
    }
    
    mockUsers["testuser"] = testUser;
    
    return new AuthService(config, null, mockUsers);
});

builder.Services.AddScoped<HerbService>(provider => new HerbService(null)); // null = usar mock
builder.Services.AddScoped<PlanService>(provider => new PlanService(null)); // null = usar mock
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUserService, UserService>();
        // ========== CONFIGURAÇÃO MOCK (100% mock, sem BD real) ==========
        builder.Services.AddScoped<IAuthService>(provider => {
            var config = provider.GetRequiredService<IConfiguration>();
            var mockUsers = new Dictionary<string, User>();
            // Hash de senha para "testpass"
            string passwordHash;
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("testpass"));
                passwordHash = Convert.ToBase64String(hashedBytes);
            }
            mockUsers["testuser"] = new User {
                Id = 1,
                Username = "testuser",
                Email = "testuser@greenherb.local",
                PasswordHash = passwordHash,
                Role = UserRole.Tecnico,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            return new AuthService(config, null, mockUsers);
        });
        builder.Services.AddScoped<HerbService>(_ => new HerbService(null));
        builder.Services.AddScoped<PlanService>(_ => new PlanService(null));
        builder.Services.AddScoped<IBatchService, BatchService>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IMeasurementService, MeasurementService>();
        builder.Services.AddScoped<IAlertService, AlertService>();
        builder.Services.AddScoped<IAutomationService, AutomationService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IUserService, UserService>();
var app = builder.Build();

// Seed de dados - criar usuário de teste (somente com BD real)
/*
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Aplicar migrações pendentes
    dbContext.Database.Migrate();
    
    // Verificar se já existe usuário testuser
    if (!dbContext.Users.Any(u => u.Username.ToLower() == "testuser"))
    {
        // Criar hash de senha "testpass" usando SHA256
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("testpass"));
            var passwordHash = Convert.ToBase64String(hashedBytes);
            
            var testUser = new User
            {
                Username = "testuser",
                Email = "testuser@greenherb.local",
                PasswordHash = passwordHash,
                Role = UserRole.Tecnico,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            dbContext.Users.Add(testUser);
            dbContext.SaveChanges();
        }
    }
}
*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
