using FinisServer.Models.Enums;

namespace FinisServer.Models.Dtos;

public record ArticleDetailDto(
    int AuthorId,
    string Title,
    DateTimeOffset CreatedOn,
    DateTimeOffset LastModifiedOn,
    string Content,
    string CategoryDisplay,
    int ViewCount,
    int CommentCount,
    int LikeCount,
    int BookmarkCount,
    bool IsLiked,
    bool IsBookmarked
    );