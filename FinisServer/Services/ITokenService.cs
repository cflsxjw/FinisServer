using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;

namespace FinisServer.Services;

public interface ITokenService
{
    public string GenerateToken(TokenCreateDto tokenCreateDto);
    
}