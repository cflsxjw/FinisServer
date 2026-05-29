using FinisServer.Interfaces;

namespace FinisServer.Models.Entities;
public class SiteNotification : IAuditEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required int AuthorId { get; set; }
    public DateTimeOffset CreatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTimeOffset { get; set; } = DateTimeOffset.UtcNow;
}