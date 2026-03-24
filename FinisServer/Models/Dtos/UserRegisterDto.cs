using System.ComponentModel.DataAnnotations;

namespace FinisServer.Models.Dtos;

public record UserRegisterDto(
    [Required]
    string Name,
    [Required]
    string Email,
    [Required]
    string Password);