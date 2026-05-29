namespace FinisServer.Services;

public interface IHistoryService
{
    Task AddHistoryAsync(long userId, string articleId);
    Task<List<int>> GetHistoryAsync(long userId);
}