using FinisServer.Services;
using StackExchange.Redis;


public class HistoryService : IHistoryService
{
    private readonly IDatabase _db;
    private const string HistoryKeyPrefix = "user:history:";
    private const int MaxHistoryCount = 100;

    // Lua 脚本：更新/插入并修剪长度
    private const string AddHistoryLua = @"
        redis.call('ZADD', KEYS[1], ARGV[1], ARGV[2])
        redis.call('ZREMRANGEBYRANK', KEYS[1], 0, -ARGV[3] - 1)
        return 1";

    public HistoryService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    /// <summary>
    /// 存入浏览历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="articleId">文章/页面ID</param>
    public async Task AddHistoryAsync(long userId, string articleId)
    {
        var key = $"{HistoryKeyPrefix}{userId}";
        var score = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // 使用毫秒时间戳保证排序精度

        await _db.ScriptEvaluateAsync(AddHistoryLua, 
            new RedisKey[] { key }, 
            new RedisValue[] { score, articleId, MaxHistoryCount });
    }

    /// <summary>
    /// 读取最近的100个历史记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>按时间倒序排列的 ID 列表</returns>
    public async Task<List<int>> GetHistoryAsync(long userId)
    {
        var key = $"{HistoryKeyPrefix}{userId}";
        
        // ZREVRANGE 返回按 Score 从大到小排列的结果（即最新的在前）
        var results = await _db.SortedSetRangeByRankAsync(key, 0, MaxHistoryCount - 1, Order.Descending);
        return results.Select(x => (int)x).ToList<int>();
    }
}