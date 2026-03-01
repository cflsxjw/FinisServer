using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FinisServer.Configurations.Database;
using FinisServer.Configurations.Options;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace FinisServer.Services.Impl;

public class TokenService(IConfiguration configuration) : ITokenService
{
    
    public string GenerateToken(TokenCreateDto tokenCreateDto)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, tokenCreateDto.Id.ToString()),
            new Claim(FinisJwtClaimTypes.Username, tokenCreateDto.Username),
            new Claim(FinisJwtClaimTypes.Role, tokenCreateDto.Role.ToString()),
        };
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: signingCredentials
        );
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(token);
    }
}