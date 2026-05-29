namespace FinisServer.Models.Dtos;

public record UserInfoDto(int Id,string Name, string Description, string Avatar, int ViewCount,  int LikeCount, int ArticleCount, int BookmarkCount);