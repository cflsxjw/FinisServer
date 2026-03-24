namespace FinisServer.Services;

public interface IQwenService
{
    public Task<IList<float[]>> EmbeddingTexts(IList<string> texts);
    public Task<IList<string>> ArticleChunkingAsync(string content);

    public Task GetResponceFromSSM(HttpContext context, object[] messages, bool enableRag);
    public Task GetResponceFromSSMWithArticle(HttpContext context, object[] messages, bool enableRag, int articleId);
}