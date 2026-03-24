namespace FinisServer.Models.Dtos;

public record class ChatRequestDto(object[] Messages, bool EnableRag);
