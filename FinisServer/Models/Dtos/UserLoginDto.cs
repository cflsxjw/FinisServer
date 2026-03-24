using System.ComponentModel.DataAnnotations;

namespace FinisServer.Models.Dtos;

public record
    UserLoginDto(
        [Required]
        string Name,
        [Required]
        string Password);