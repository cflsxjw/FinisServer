using System.ComponentModel.DataAnnotations;

namespace FinisServer.Models.Dtos;

public record UserSettingDto(
    [Required]
    string Name,
    [Required]
    string Email,
    string? Description = null,
    string? Avatar = null
    );