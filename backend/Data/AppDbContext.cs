using Microsoft.EntityFrameworkCore;
using OcrDemo.Api.Models;

namespace OcrDemo.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OcrTask> OcrTasks => Set<OcrTask>();

    public DbSet<OcrTaskHistory> OcrTaskHistories => Set<OcrTaskHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OcrTask>(entity =>
        {
            entity.ToTable("ocr_tasks");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.RawJsonBlobPath)
                  .HasMaxLength(500)
                  .IsRequired();

            entity.Property(e => e.ResultJsonBlobPath)
                  .HasMaxLength(500)
                  .IsRequired();

            entity.Property(e => e.Status)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<OcrTaskHistory>(entity =>
        {
            entity.ToTable("ocr_task_histories");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.BlobPath)
                  .HasMaxLength(500)
                  .IsRequired();

            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(100);

            entity.HasIndex(e => e.TaskId);
        });
    }
}