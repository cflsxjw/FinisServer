using FinisServer.Models.Enums;

namespace FinisServer.Models.Entities;

public class ArticleLikeRecord
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required int ArticleId { get; set; }

    public Article Article { get; set; } = null!;
}