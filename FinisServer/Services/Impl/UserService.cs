using FinisServer.Configurations.Database;
using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using FinisServer.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinisServer.Services.Impl;

public class UserService(FinisDbContext finisDbContext, ITokenService tokenService, IFinisHttpContext finisHttpContext) : IUserService
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

    public async Task<UserInfoDto> GetUserInfoAsync(int id)
    {
        User user = await finisDbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new ResourceNotFoundException("用户不存在");
        var dto = new UserInfoDto(
            Name: user.Name,
            Description: user.Description ?? string.Empty,
            ViewCount:  user.ViewCount,
            LikeCount: user.LikeCount,
            ArticleCount:  user.ArticleCount,
            Avatar: user.Avatar);
        return dto;
    }

    public async Task<UserSettingDto> GetUserSettingAsync()
    {
        var requestUserId = finisHttpContext.GetRequestUserId();
        User user = await finisDbContext.Users
                .FirstOrDefaultAsync(u => u.Id == requestUserId)
            ?? throw new ResourceNotFoundException("用户不存在");
        return new UserSettingDto(Name: user.Name, Email: user.Email, Description: user.Description);
    }

    public async Task UpdateUserSettingAsync(UserSettingDto userSettingDto)
    {
        var requestUserId = finisHttpContext.GetRequestUserId();
        User user = await finisDbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == requestUserId)
                    ?? throw new ResourceNotFoundException("用户不存在");
        user.Name = userSettingDto.Name;
        user.Avatar = userSettingDto.Avatar ?? user.Avatar;
        user.Description = userSettingDto.Description ?? user.Description;
        user.Email = userSettingDto.Email;
        await finisDbContext.SaveChangesAsync();
    }
}