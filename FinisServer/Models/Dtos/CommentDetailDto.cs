namespace FinisServer.Models.Dtos;

public record CommentDetailDto(
    int Id,
    string Content,
    int AuthorId,
    int ArticleId,
    DateTimeOffset CreateOn,
    DateTimeOffset LastModifiedOn,
    List<CommentDetailDto>? SubComments = null,
    int? RootCommentId = null,
    int? ReplyToUserId = null);