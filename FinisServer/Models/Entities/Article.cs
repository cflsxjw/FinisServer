using FinisServer.Interfaces;

namespace FinisServer.Models.Entities;

public class Article : IAuditEntity
{
    /// <summary>
    /// 文章 ID
    /// <remarks>
    /// [数据库] Post 主键、自增
    /// </remarks>
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 文章标题
    /// <remarks>
    /// [数据库]非空、最大长度 64
    /// </remarks>
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 文章摘要
    /// <remarks>
    /// [数据库]非空、最大长度 256
    /// </remarks>
    /// </summary>
    public string Summary { get; set; } = null!;
    
    /// <summary>
    /// 阅读数
    /// </summary>
    public int ViewCount { get; set; }
    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; }
    /// <summary>
    /// 点踩数
    /// </summary>
    public int DislikeCount { get; set; }
    /// <summary>
    /// 作者 ID
    /// </summary>
    public int AuthorId { get; set; }
    public DateTimeOffset CreatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public virtual User Author { get; set; } = null!;
    public virtual ArticleContent Content { get; set; } = null!;
}