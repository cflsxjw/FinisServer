using FinisServer.Models.Dtos;
namespace FinisServer.Services;
public interface IRankingService
{
    // 记录点击
    Task RecordClickAsync(int articleId, int authorId);
    
    // 清理 24 小时前的过期数据
    Task ClearExpiredTicksAsync();

    // 获取文章 Top 10 ID 列表
    Task<List<ArticleInfoDto>> GetTopArticlesAsync(int limit = 5);

    // 获取作者 Top 10 ID 列表
    Task<List<UserInfoDto>> GetTopAuthorsAsync(int limit = 5);
    Task DeleteArticleRankAsync(int articleId, int authorId);
}