using FinisServer.Configurations.Database;
using FinisServer.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
namespace FinisServer.Services.Impl;

public class RankingService(IConnectionMultiplexer redis, FinisDbContext finisDbContext) : IRankingService
{
    private readonly IDatabase _redisDatabase = redis.GetDatabase();

    private const string ClickLogKey = "click:log";     // ZSET: [member: articleId:authorId:ts, score: timestamp]
    private const string ArticleRankKey = "rank:clicks"; // ZSET: [member: articleId, score: count]
    private const string AuthorRankKey = "rank:authors"; // ZSET: [member: authorId, score: count]

    // 1. 记录点击
    public async Task RecordClickAsync(int articleId, int authorId)
    {
        await ClearExpiredTicksAsync();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // 增加毫秒级后缀和短随机数，确保 member 唯一性
        var member = $"{articleId}:{authorId}:{now}:{Guid.NewGuid().ToString("N")[..4]}";

        var batch = _redisDatabase.CreateBatch();
        // 记录流水用于清理
        var t1 = batch.SortedSetAddAsync(ClickLogKey, member, now / 1000);
        // 实时增加权重
        var t2 = batch.SortedSetIncrementAsync(ArticleRankKey, articleId.ToString(), 1);
        var t3 = batch.SortedSetIncrementAsync(AuthorRankKey, authorId.ToString(), 1);

        batch.Execute();
        await Task.WhenAll(t1, t2, t3);
    }

    // 2. 清理过期数据（推荐在后台 Job 中每分钟执行一次）
    public async Task ClearExpiredTicksAsync()
    {
        var dayAgo = DateTimeOffset.UtcNow.AddHours(-24).ToUnixTimeSeconds();

        string luaScript = File.ReadAllText("Configurations/Database/Redis/ClearExpiredTicks.lua");

        await _redisDatabase.ScriptEvaluateAsync(luaScript,
            [(RedisKey)ClickLogKey, (RedisKey)ArticleRankKey, (RedisKey)AuthorRankKey],
            [(RedisValue)dayAgo]);
    }

    public async Task<List<ArticleInfoDto>> GetTopArticlesAsync(int limit)
    {
        var results = await _redisDatabase.SortedSetRangeByRankWithScoresAsync(
            ArticleRankKey, 0, limit - 1, Order.Descending);

        if (results.Length == 0) return [];

        var scoreMap = results.ToDictionary(
            e => int.Parse(e.Element!),
            e => (int)e.Score
        );
        var ids = scoreMap.Keys.ToList();

        var articles = await finisDbContext.Articles
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();

        return [.. articles
            
            .OrderByDescending(a => scoreMap[a.Id])
            .ThenByDescending(a => a.CreatedTimeOffset)
            .Select(a => new ArticleInfoDto
            (
                Id: a.Id,
                Title: a.Title,
                Summary: a.Summary,
                ArticleCategory: a.Category,
                ViewCount: a.ViewCount,
                LikesCount: a.LikeCount,
                BookmarkCount: a.BookmarkCount,
                CommentCount: a.CommentCount,
                AuthorId: a.AuthorId,
                CreateOn: a.CreatedTimeOffset,
                LastModifiedOn: a.UpdatedTimeOffset,
                CoverPath: a.CoverPath
            ))];
    }

    public async Task<List<UserInfoDto>> GetTopAuthorsAsync(int limit)
    {
        var results = await _redisDatabase.SortedSetRangeByRankWithScoresAsync(
            AuthorRankKey, 0, limit - 1, Order.Descending);

        if (results.Length == 0) return [];

        var scoreMap = results.ToDictionary(
            e => int.Parse(e.Element!),
            e => (int)e.Score
        );
        var ids = scoreMap.Keys.ToList();

        var users = await finisDbContext.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();

        return [.. users
            .OrderByDescending(u => scoreMap[u.Id])
            .Select(u => new UserInfoDto(
                Id: u.Id,
                Name: u.Name,
                Description: u.Description ?? "",
                Avatar: u.Avatar,
                ViewCount: u.ViewCount,
                LikeCount: u.LikeCount,
                ArticleCount: u.ArticleCount,
                BookmarkCount: u.BookmarkCount
            ))];
    }
    public async Task DeleteArticleRankAsync(int articleId, int authorId)
    {
        var currentScore = await _redisDatabase.SortedSetScoreAsync(ArticleRankKey, articleId.ToString());
        if (currentScore == null) return;
        var batch = _redisDatabase.CreateBatch();
        var t1 = batch.SortedSetRemoveAsync(ArticleRankKey, articleId.ToString());
        var t2 = batch.SortedSetIncrementAsync(AuthorRankKey, authorId.ToString(), -currentScore.Value);
        batch.Execute();
        await Task.WhenAll(t1, t2);
    }
}