using System;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Entities;

public class CommentLikeRecord
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required int CommentId { get; set; }

    public Comment Comment { get; set; } = null!;
}
