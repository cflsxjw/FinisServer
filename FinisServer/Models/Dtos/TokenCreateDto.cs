using FinisServer.Models.Entities;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Dtos;

public record TokenCreateDto(int Id, string Username, UserRole Role);