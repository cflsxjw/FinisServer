using System.ComponentModel.DataAnnotations;
using FinisServer.Models.Enums;

namespace FinisServer.Models.Dtos;

public record ArticlePostDto(
    [Required]
    ArticleCategory Category,
    [Required]
    string Title,
    [Required]
    string Summary,
    [Required]
    string Content,
    string? CoverPath,
    string[] Keywords);