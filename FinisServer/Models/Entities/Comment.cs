namespace FinisServer.Models.Entities;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public int LikeCount { get; set; }
    public int AuthorId { get; set; }

    public int ArticleId { get; set; }
    public int? RootCommentId { get; set; }
    public int? ReplyToUserId { get; set; }
    public bool IsDeletedByAdmin { get; set; } = false;
    public DateTimeOffset CreatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public virtual User Author { get; set; } = null!;
    public virtual Article Article { get; set; } = null!;
    public virtual Comment? RootComment { get; set; }
    public virtual ICollection<Comment>? Replies { get; set; }
}