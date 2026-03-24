namespace FinisServer.Models.Dtos;

public record UserInfoDto(string Name, string Description, string Avatar, int ViewCount,  int LikeCount, int ArticleCount);