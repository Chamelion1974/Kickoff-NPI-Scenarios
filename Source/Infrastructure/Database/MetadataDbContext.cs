using Microsoft.EntityFrameworkCore;
using ShopStewardHub.DigitalTwin.Models;

namespace ShopStewardHub.DigitalTwin.Infrastructure.Database;

/// <summary>
/// Entity Framework DbContext for metadata database (PostgreSQL)
/// </summary>
public class MetadataDbContext : DbContext
{
    public MetadataDbContext(DbContextOptions<MetadataDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<JobRouting> JobRoutings => Set<JobRouting>();
    public DbSet<Operation> Operations => Set<Operation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50)
                .HasConversion<string>().IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SizeX).HasColumnName("size_x");
            entity.Property(e => e.SizeY).HasColumnName("size_y");
            entity.Property(e => e.SizeZ).HasColumnName("size_z");
            entity.Property(e => e.PositionX).HasColumnName("position_x");
            entity.Property(e => e.PositionY).HasColumnName("position_y");
            entity.Property(e => e.PositionZ).HasColumnName("position_z");
            entity.Property(e => e.Rotation).HasColumnName("rotation");
            entity.Property(e => e.MaxMachines).HasColumnName("max_machines");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(e => e.Machines)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Machine configuration
        modelBuilder.Entity<Machine>(entity =>
        {
            entity.ToTable("machines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.MachineNumber).HasColumnName("machine_number").HasMaxLength(50);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50)
                .HasConversion<string>().IsRequired();
            entity.Property(e => e.Model).HasColumnName("model").HasMaxLength(100);
            entity.Property(e => e.Manufacturer).HasColumnName("manufacturer").HasMaxLength(100);
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number").HasMaxLength(100);
            entity.Property(e => e.InstallationDate).HasColumnName("installation_date");
            entity.Property(e => e.LastMaintenanceDate).HasColumnName("last_maintenance_date");
            entity.Property(e => e.NextMaintenanceDate).HasColumnName("next_maintenance_date");
            entity.Property(e => e.PositionX).HasColumnName("position_x");
            entity.Property(e => e.PositionY).HasColumnName("position_y");
            entity.Property(e => e.PositionZ).HasColumnName("position_z");
            entity.Property(e => e.Rotation).HasColumnName("rotation");
            entity.Property(e => e.MaxSpindleSpeed).HasColumnName("max_spindle_speed");
            entity.Property(e => e.MaxFeedRate).HasColumnName("max_feed_rate");
            entity.Property(e => e.WorkEnvelopeX).HasColumnName("work_envelope_x");
            entity.Property(e => e.WorkEnvelopeY).HasColumnName("work_envelope_y");
            entity.Property(e => e.WorkEnvelopeZ).HasColumnName("work_envelope_z");
            entity.Property(e => e.NumberOfAxes).HasColumnName("number_of_axes");
            entity.Property(e => e.HasToolChanger).HasColumnName("has_tool_changer");
            entity.Property(e => e.ToolCapacity).HasColumnName("tool_capacity");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsMaintenance).HasColumnName("is_maintenance");
            entity.Property(e => e.ControllerType).HasColumnName("controller_type").HasMaxLength(50);
            entity.Property(e => e.ControllerIp).HasColumnName("controller_ip").HasMaxLength(45);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Job Routing configuration
        modelBuilder.Entity<JobRouting>(entity =>
        {
            entity.ToTable("job_routings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobNumber).HasColumnName("job_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PartNumber).HasColumnName("part_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Revision).HasColumnName("revision").HasMaxLength(10);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.QuantityCompleted).HasColumnName("quantity_completed");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.JobType).HasColumnName("job_type").HasMaxLength(20)
                .HasConversion<string>().IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20)
                .HasConversion<string>().IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ReleasedAt).HasColumnName("released_at");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.EstimatedTotalHours).HasColumnName("estimated_total_hours");
            entity.Property(e => e.ActualTotalHours).HasColumnName("actual_total_hours");
            entity.Property(e => e.IsItar).HasColumnName("is_itar");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(e => e.Operations)
                .WithOne(e => e.Routing)
                .HasForeignKey(e => e.RoutingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Operation configuration
        modelBuilder.Entity<Operation>(entity =>
        {
            entity.ToTable("operations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoutingId).HasColumnName("routing_id");
            entity.Property(e => e.Sequence).HasColumnName("sequence");
            entity.Property(e => e.OperationCode).HasColumnName("operation_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.EstimatedSetupHours).HasColumnName("estimated_setup_hours");
            entity.Property(e => e.EstimatedCycleHours).HasColumnName("estimated_cycle_hours");
            entity.Property(e => e.ActualSetupHours).HasColumnName("actual_setup_hours");
            entity.Property(e => e.ActualCycleHours).HasColumnName("actual_cycle_hours");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20)
                .HasConversion<string>().IsRequired();
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ProgramNumber).HasColumnName("program_number").HasMaxLength(50);
            entity.Property(e => e.ProgramPath).HasColumnName("program_path");
            entity.Property(e => e.GcodePath).HasColumnName("gcode_path");
            entity.Property(e => e.RequiresFirstArticle).HasColumnName("requires_first_article");
            entity.Property(e => e.FirstArticleCompleted).HasColumnName("first_article_completed");
            entity.Property(e => e.InspectionRequired).HasColumnName("inspection_required");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Machine)
                .WithMany()
                .HasForeignKey(e => e.MachineId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
