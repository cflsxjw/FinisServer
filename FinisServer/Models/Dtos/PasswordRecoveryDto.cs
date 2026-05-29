namespace FinisServer.Models.Dtos;
using System.ComponentModel.DataAnnotations;
public record PasswordRecoveryDto(
    [Required]
    string Username,
    [Required]
    string Password,
    [Required]
    string Answer);