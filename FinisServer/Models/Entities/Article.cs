using FinisServer.Interfaces;
using FinisServer.Models.Enums;

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
    public required string Title { get; set; }

    /// <summary>
    /// 文章摘要
    /// <remarks>
    /// [数据库]非空、最大长度 128
    /// </remarks>
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// 文章分类
    /// </summary>
    public ArticleCategory Category { get; set; }

    /// <summary>
    /// 阅读数
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// 收藏数
    /// </summary>
    public int BookmarkCount { get; set; } = 0;
    /// <summary>
    ///
    /// </summary>
    public int CommentCount { get; set; } = 0;
    /// <summary>
    /// 作者 ID
    /// </summary>
    public int AuthorId { get; set; }
    /// <summary>
    /// 封面路径
    /// [数据库] 最大长度 256
    /// </summary>
    public string? CoverPath { get; set; }


    public DateTimeOffset CreatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public virtual User Author { get; set; } = null!;
    public virtual ArticleContent Content { get; set; } = null!;
}