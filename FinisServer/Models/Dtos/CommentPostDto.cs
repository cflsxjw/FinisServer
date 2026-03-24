using System.ComponentModel.DataAnnotations;

namespace FinisServer.Models.Dtos;

public record CommentPostDto(
    [Required]
    string Content,
    int ArticleId,
    int? RootCommentId,
    int? ReplyToUserId);