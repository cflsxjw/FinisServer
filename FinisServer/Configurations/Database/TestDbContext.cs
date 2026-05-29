using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestArticle> TestArticles { get; set; }
    public DbSet<TestTag> TestTags { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestArticle>(entity =>
        {
            // 指定表名
            entity.ToTable("article");

            // 设置主键
            entity.HasKey(e => e.JuejinId);

            // 批量配置非空约束 (Not Null)
            entity.Property(e => e.JuejinId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PublishDate).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Keywords).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(255);

            // 整数类型配置
            entity.Property(e => e.ViewCount).IsRequired();
            entity.Property(e => e.ReadTime).IsRequired();
        });
        modelBuilder.Entity<TestTag>(entity =>
        {
            // 映射到表名 tag
            entity.ToTable("tag");
            // 主键配置
            entity.HasKey(e => e.TagId);
            // 字段约束
            entity.Property(e => e.TagId)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.TagName)
                .IsRequired()
                .HasMaxLength(255);

            // 布尔值及默认值配置
            entity.Property(e => e.Fetched)
                .HasDefaultValue(false); // 对应 SQLite 中的 default: 0
        });
    }
}