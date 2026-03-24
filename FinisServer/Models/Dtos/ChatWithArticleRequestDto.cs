namespace FinisServer.Models.Dtos;

public record class ChatWithArticleRequestDto(object[] Messages, bool EnableRag, int ArticleId) : ChatRequestDto(Messages, EnableRag);
