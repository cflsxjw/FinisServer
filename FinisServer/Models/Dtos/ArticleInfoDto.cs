using System.ComponentModel.DataAnnotations;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Dtos;

public record ArticleInfoDto(
    int Id,
    string Title,
    string Summary,
    ArticleCategory ArticleCategory,
    int ViewCount,
    int LikesCount,
    int BookmarkCount,
    int CommentCount,
    int AuthorId,
    DateTimeOffset CreateOn,
    DateTimeOffset LastModifiedOn,
    string? CoverPath = null);