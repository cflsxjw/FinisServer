using FinisServer.Configurations.Database.Interceptors;
using FinisServer.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinisServer.Configurations.Database;

public class FinisDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public FinisDbContext(DbContextOptions<FinisDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // UTC 转换器
        var utcConverter = new ValueConverter<DateTimeOffset, DateTime>(
            time => time.UtcDateTime,
            time => new DateTimeOffset(time, TimeSpan.Zero)
        );
        // Entities 定义
        modelBuilder.Entity<User>(entity => 
        {
            // 表名
            entity.ToTable("user");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>();
            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(u => u.Avatar)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(u => u.Description)
                .HasMaxLength(32);
            entity.Property(u => u.CreatedTimeOffset)
                .IsRequired()
                .HasColumnType("datetime(6)")
                .HasConversion(utcConverter);
            entity.Property(u => u.UpdatedTimeOffset)
                .IsRequired()
                .HasColumnType("datetime(6)")
                .HasConversion(utcConverter);
            entity.Property(u => u.LastActiveTimeOffset)
                .IsRequired()
                .HasColumnType("datetime(6)")
                .HasConversion(utcConverter);
            // 索引
            entity.HasIndex(u => u.Name)
                .IsUnique();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new TimeInterceptor());
    }
}