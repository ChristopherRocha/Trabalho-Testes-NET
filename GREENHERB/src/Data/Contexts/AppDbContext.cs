using Microsoft.EntityFrameworkCore;
using GREENHERB.src.Models;

namespace GREENHERB.src.Data.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Herb> Herbs { get; set; }
    public DbSet<CultivationPlan> CultivationPlans { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<OperationalTask> OperationalTasks { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Automation> Automations { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Suprimir aviso de pending model changes para evitar problemas com migrações
        optionsBuilder.ConfigureWarnings(w => 
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações da entidade User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            
            entity.Property(e => e.PasswordHash)
                .IsRequired();
            
            entity.Property(e => e.Role)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => e.Username)
                .IsUnique();
            
            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Herb>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ScientificName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Origin)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Notes)
                .HasMaxLength(2000);

            entity.Property(e => e.CareInstructions)
                .HasMaxLength(2000);

            entity.Property(e => e.CycleDays)
                .IsRequired();

            entity.HasIndex(e => new { e.Name, e.ScientificName })
                .IsUnique();
        });

        modelBuilder.Entity<CultivationPlan>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.DurationDays)
                .IsRequired();

            entity.Property(e => e.WateringFrequencyDays)
                .IsRequired();

            entity.Property(e => e.Notes)
                .HasMaxLength(2000);

            entity.HasOne(e => e.Herb)
                .WithMany(h => h.CultivationPlans)
                .HasForeignKey(e => e.HerbId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações da entidade Batch
        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LossPercentage)
                .HasPrecision(5, 2);

            entity.Property(e => e.Productivity)
                .HasPrecision(10, 2);

            entity.HasOne<CultivationPlan>()
                .WithMany()
                .HasForeignKey(e => e.CultivationPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações da entidade Task
        modelBuilder.Entity<OperationalTask>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TaskType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne<Batch>()
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurações da entidade Measurement
        modelBuilder.Entity<Measurement>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Temperature)
                .HasPrecision(5, 2);

            entity.Property(e => e.Humidity)
                .HasPrecision(5, 2);

            entity.Property(e => e.Luminosity)
                .HasPrecision(10, 2);

            entity.HasOne<Batch>()
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações da entidade Alert
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.AlertType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Resolution)
                .HasMaxLength(2000);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.ResolvedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurações da entidade Automation
        modelBuilder.Entity<Automation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.TriggerCondition)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.OperationMode)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne<Batch>()
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações da entidade Report
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.ReportType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ExportFormat)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FilePath)
                .HasMaxLength(500);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurações da entidade AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OperationType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.OldValues)
                .HasMaxLength(4000);

            entity.Property(e => e.NewValues)
                .HasMaxLength(4000);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(100);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}


