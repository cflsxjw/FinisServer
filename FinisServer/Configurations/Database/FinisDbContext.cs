using FinisServer.Configurations.Database.Interceptors;
using FinisServer.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinisServer.Configurations.Database;

public class FinisDbContext(DbContextOptions<FinisDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleContent> ArticleContents { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ArticleVector> ArticleVectors { get; set; }
    public DbSet<ArticleLikeRecord> ArticleLikeRecords { get; set; }
    public DbSet<ArticleBookmarkRecord> ArticleBookmarkRecords { get; set; }
    public DbSet<CommentLikeRecord> CommentLikeRecords { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UserFollowRecord> UserFollowRecords { get; set; }
    public DbSet<SiteNotification> SiteNotifications { get; set; }
    public DbSet<ReportRecord> ReportRecords { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("pg_search");

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
                .IsRequired();
            entity.Property(u => u.UpdatedTimeOffset)
                .IsRequired();
            entity.Property(u => u.LastActiveTimeOffset)
                .IsRequired();
            // 索引
            entity.HasIndex(u => u.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Article>(entity =>
        {
            // 表名
            entity.ToTable("article");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Title)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(u => u.Summary)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(u => u.Category)
                .IsRequired()
                .HasConversion<string>();
            entity.HasIndex(u => u.Title);
            entity.HasIndex(u => u.AuthorId);
            entity.Property(u => u.CreatedTimeOffset)
                .IsRequired();
            entity.Property(u => u.UpdatedTimeOffset)
                .IsRequired();
            entity.Property(u => u.CoverPath)
                .HasMaxLength(256);
            entity.Property(u => u.Keywords)
                .IsRequired();
            
            modelBuilder.Entity<Article>()
                .HasIndex(a => a.Title)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
            
            entity.HasMany(u => u.ArticleVectors)
                .WithOne(u => u.ParentArticle)
                .HasForeignKey(u => u.ParentArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ArticleContent>(entity =>
        {
            // 表名
            entity.ToTable("article_content");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Content)
                .IsRequired()
                .HasMaxLength(65535)
                .HasColumnType("TEXT");
            entity.HasIndex(u => u.ArticleId);
            entity.HasIndex(a => new { a.ArticleId, a.Content }, "IX_article_content_search")
            .HasMethod("bm25")
            .HasStorageParameter("key_field", "ArticleId")
            .HasStorageParameter("text_fields", @"{
                ""Content"": {
                    ""tokenizer"": { ""type"": ""jieba"", ""mode"": ""search"" },
                    ""record"": ""position""
                }
            }");
        });
        modelBuilder.Entity<Comment>(entity =>
        {
            // 表名
            entity.ToTable("comment");
            // Key
            entity.HasKey(u => u.Id);
            // 外键
            entity.HasMany(u => u.Replies)
                .WithOne(u => u.RootComment)
                .HasForeignKey(u => u.RootCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Content)
                .IsRequired()
                .HasMaxLength(1024);
            entity.Property(u => u.CreatedTimeOffset)
                .IsRequired();
            entity.Property(u => u.UpdatedTimeOffset)
                .IsRequired();
        });
        modelBuilder.Entity<ArticleVector>(entity =>
        {
            // 表名
            entity.ToTable("article_vector");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Content)
                .IsRequired()
                .HasMaxLength(1024);
            entity.Property(u => u.Embedding)
                .IsRequired()
                .HasColumnType("vector(1024)");
            entity.HasIndex(a => new { a.Id, a.Content }, "IX_article_content_search")
            .HasMethod("bm25")
            .HasStorageParameter("key_field", "Id")
            .HasStorageParameter("text_fields", @"{
                ""Content"": {
                    ""tokenizer"": { ""type"": ""jieba"", ""mode"": ""search"" },
                    ""record"": ""position""
                }
            }");
        });
        modelBuilder.Entity<ArticleLikeRecord>(entity =>
        {
            // 表名
            entity.ToTable("article_like_record");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.ArticleId)
                .IsRequired();
            entity.Property(u => u.UserId)
                .IsRequired();
        });
        modelBuilder.Entity<ArticleBookmarkRecord>(entity =>
        {
            // 表名
            entity.ToTable("article_bookmark_record");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.ArticleId)
                .IsRequired();
            entity.Property(u => u.UserId)
                .IsRequired();
        });
        modelBuilder.Entity<CommentLikeRecord>(entity =>
        {
            // 表名
            entity.ToTable("comment_like_record");
            // Key
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
               .ValueGeneratedOnAdd();
            entity.Property(u => u.CommentId)
                .IsRequired();
            entity.Property(u => u.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tag");
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Name)
                .IsRequired();
        });

        modelBuilder.Entity<UserFollowRecord>(entity =>
        {
            entity.ToTable("user_follow_record");
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.UserId)
                .IsRequired();
            entity.Property(u => u.FollowedUserId)
                .IsRequired();
        });
        modelBuilder.Entity<SiteNotification>(entity =>
        {
            entity.ToTable("site_notification");
            entity.HasKey(u => u.Id);
            // 字段属性
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.Title)
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(u => u.Content)
                .HasMaxLength(2048)
                .IsRequired();
            entity.Property(u => u.AuthorId)
                .IsRequired();
        });
        modelBuilder.Entity<ReportRecord>(entity =>
        {
            entity.ToTable("report_record");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();
            entity.Property(u => u.ReportType)
                .IsRequired()
                .HasConversion<string>();
            entity.Property(u => u.IsDone)
                .IsRequired();
            entity.Property(u => u.Reason)
                .HasMaxLength(256)
                .IsRequired();
            entity.Property(u => u.AdminId);
            entity.Property(u => u.TargetIdentifierId)
                .IsRequired();
            entity.Property(u => u.ReporterId)
                .IsRequired();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new TimeInterceptor());
    }

    
}