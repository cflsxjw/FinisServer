using FinisServer.Models.Dtos;

namespace FinisServer.Services;

public interface IQwenService
{
    public Task<IList<float[]>> EmbeddingTextsAsync(IList<string> texts);
    public IList<string> ArticleChunking(string content, string title, bool attachTitle = true);

    public Task GetResponseFromSSM(HttpContext context, object[] messages, bool enableRag);
    public Task GetResponseFromSSMWithArticle(HttpContext context, object[] messages, bool enableRag, int articleId);

    public Task<(string[] Contents, int[] ArticleIds)> RagQueryAsync(string[] keywords);
}