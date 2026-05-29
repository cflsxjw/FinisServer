using System.Security.Permissions;
using FinisServer.Interfaces;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Entities;

public class User : IAuditEntity
{ 
    /// <summary>
    /// 用户 ID
    /// <remarks>
    /// [数据库]自增、User表主键
    /// </remarks>
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// 用户角色
    /// </summary>
    public UserRole Role { get; set; }
    /// <summary>
    /// 用户名
    /// <remarks>
    /// [数据库]非空、最大长度 32
    /// </remarks>
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// 头像路径
    /// <remarks>
    /// [数据库]非空、最大长度 256
    /// </remarks>
    /// </summary>
    public required string Avatar { get; set; }
    /// <summary>
    /// 邮箱
    /// <remarks>
    /// [数据库]非空、最大长度 64
    /// </remarks>
    /// </summary>
    public required string Email { get; set; }
    /// <summary>
    /// BCrypt 密码哈希
    /// <remarks>
    /// [数据库]非空、最大长度 64
    /// </remarks>
    /// </summary>
    public required string PasswordHash { get; set; }
    /// <summary>
    /// 个人签名
    /// <remarks>
    /// [数据库]非空、最大长度 32
    /// </remarks>
    /// </summary>
    public string? Description {get; set;}
    /// <summary>
    /// 创建时间
    /// <remarks>
    /// [数据库]非空、DATETIME(6)
    /// </remarks>
    /// </summary>
    public DateTimeOffset CreatedTimeOffset { get; set; }
    /// <summary>
    /// 修改时间
    /// <remarks>
    /// [数据库]非空、DATETIME(6)
    /// </remarks>
    /// </summary>
    public DateTimeOffset UpdatedTimeOffset { get; set; }
    /// <summary>
    /// 上次登录时间
    /// <remarks>
    /// [数据库]非空、DATETIME(6)
    /// </remarks>
    /// </summary>
    public DateTimeOffset LastActiveTimeOffset { get; set; }

    public string SecurityQuestion { get; set; }
    public string SecurityAnswerHash { get; set; }
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public int BookmarkCount { get; set; } = 0;
    public int ArticleCount { get; set; } = 0;
}