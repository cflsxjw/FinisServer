using FinisServer.Models.Entities;

namespace FinisServer.Models.Dtos;

public record TokenCreateDto(int Id, string Username, UserRole Role);