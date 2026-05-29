using Pgvector;

namespace FinisServer.Models.Entities;

public class ArticleVector
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public required Vector Embedding { get; set; }

    public int ParentArticleId { get; set; }
    public Article ParentArticle { get; set; } = null!;
}