using FinisServer.Configurations.Database;
using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinisServer.Services.Impl;

public class UserService(FinisDbContext finisDbContext, ITokenService tokenService) : IUserService
{

    public async Task<string> SayHelloAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException("无效的用户id");
        }
        return "Hello World!";
    }

    public async Task RegisterAsync(UserRegisterDto userRegisterDto)
    {
        User newUser = new User()
        {
            Name = userRegisterDto.Name,
            Email = userRegisterDto.Email,
            Role = UserRole.User,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
            Avatar = "DefaultAvatar.jpeg"
        };
        finisDbContext.Add(newUser);
        await finisDbContext.SaveChangesAsync();
    }

    public async Task<string> LoginAsync(UserLoginDto userLoginDto)
    {
        User user = await finisDbContext.Users
                        .FirstOrDefaultAsync(u => u.Name == userLoginDto.Name) 
                    ?? throw new AuthenticationException("用户名或密码不正确");
        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
        {
            throw new AuthenticationException("用户名或密码不正确");
        }

        TokenCreateDto tokenCreateDto = new(user.Id, user.Name, user.Role);
        return tokenService.GenerateToken(tokenCreateDto);
    }
}