namespace FinisServer.Models.Entities;

public class ArticleContent
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public int ArticleId { get; set; }

    public Article Article { get; set; } = null!;
}