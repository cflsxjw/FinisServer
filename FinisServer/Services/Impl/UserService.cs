using FinisServer.Configurations.Database;
using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using FinisServer.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinisServer.Services.Impl;

public class UserService(FinisDbContext finisDbContext, ITokenService tokenService, IFinisHttpContext finisHttpContext, IHistoryService historyService) : IUserService
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
            Avatar = "DefaultAvatar.jpeg",
            SecurityQuestion = userRegisterDto.SecurityQuestion,
            SecurityAnswerHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.SecurityAnswer)
        };
        finisDbContext.Add(newUser);
        await finisDbContext.SaveChangesAsync();
    }
    
    public async Task FollowUserAsync(int id)
    {
        var currentUserId = finisHttpContext.GetRequestUserId() ?? throw new AuthenticationException();
        if (currentUserId == id)
        {
            throw new BusinessException("不能关注自己");
        }

        var record = await finisDbContext.UserFollowRecords
            .FirstOrDefaultAsync(r => r.UserId == currentUserId && r.FollowedUserId == id);

        if (record != null)
        {
            finisDbContext.UserFollowRecords.Remove(record);
        }
        else
        {
            var targetUser = await finisDbContext.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new ResourceNotFoundException("目标用户不存在");

            finisDbContext.UserFollowRecords.Add(new UserFollowRecord { UserId = currentUserId, FollowedUserId = id });
        }
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
            Id: user.Id,
            Name: user.Name,
            Description: user.Description ?? string.Empty,
            ViewCount:  user.ViewCount,
            LikeCount: user.LikeCount,
            ArticleCount:  user.ArticleCount,
            BookmarkCount: user.BookmarkCount,
            Avatar: user.Avatar);
        return dto;
    }
    public async Task RecoveryAsync(PasswordRecoveryDto passwordRecoveryDto)
    {
        var user = await finisDbContext.Users
                    .Where(u => u.Name == passwordRecoveryDto.Username)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("用户不存在");
        if (BCrypt.Net.BCrypt.Verify(passwordRecoveryDto.Answer, user.SecurityAnswerHash))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordRecoveryDto.Password);
        }
        else
        {
            throw new OperationNotAllowedException("验证失败");
        }
        await finisDbContext.SaveChangesAsync();
    }

    public async Task<bool> HasFollowUserAsync(int id)
    {
        var userid = finisHttpContext.GetRequestUserId();
        var record = await finisDbContext.UserFollowRecords
                        .Where(r => r.UserId == userid && r.FollowedUserId == id)
                        .FirstOrDefaultAsync();
        return record != null;
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

    public async Task<List<int>> GetHistory()
    {
        int userId = finisHttpContext.GetRequestUserId() ?? throw new BusinessException("用户不存在");
        var articles = await historyService.GetHistoryAsync(userId);
        return articles;
    }

    public async Task<string> GetSecurityQuestion(string username)
    {
        var question = await finisDbContext.Users
                    .Where(u => u.Name == username)
                    .Select(u => u.SecurityQuestion)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("用户不存在");
        return question;
    }
}