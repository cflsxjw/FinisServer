namespace FinisServer.Models.Entities;

public class Comment
{
    public int Id { get; set; }
    // public string Content { get; set; }
    public int LikeCount { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTimeOffset CreatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public virtual User Author { get; set; } = null!;
    public virtual Article Article { get; set; } = null!;
    public virtual Comment? Parent { get; set; }
    public virtual ICollection<Comment>? Replies { get; set; }
}