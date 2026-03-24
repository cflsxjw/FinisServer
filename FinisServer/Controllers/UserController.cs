using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("hello/{id:int}")]
    public async Task<Result<string>> Hello(int id)
    {
        var text = await userService.SayHelloAsync(id);
        return Result<string>.Success(text);
    }

    [HttpPost("register")]
    public async Task<Result> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        await userService.RegisterAsync(userRegisterDto);
        return Result.Success("注册成功");
    }
    
    [HttpPost("login")]
    public async Task<Result<TokenDto>> Login([FromBody] UserLoginDto userLoginDto)
    {
        var token = await userService.LoginAsync(userLoginDto);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(2)
        };
        Response.Cookies.Append("access_token", token, cookieOptions);
        return Result<TokenDto>.Success(new TokenDto(token), "登录成功");
    }

    [Authorize]
    [HttpPost("logout")]
    public Result Logout()
    {
        Response.Cookies.Delete("access_token");
        return Result.Success("登出成功");
    }

    [HttpGet("info/{id:int}")]
    public async Task<Result<UserInfoDto>> GetUserInfo(int id)
    {
        var res = await userService.GetUserInfoAsync(id);
        return Result<UserInfoDto>.Success(res);
    }

    [Authorize]
    [HttpGet("setting")]
    public async Task<Result<UserSettingDto>> GetUserSetting()
    {
        var res = await userService.GetUserSettingAsync();
        return Result<UserSettingDto>.Success(res);
    }

    [Authorize]
    [HttpPost("setting/update")]
    public async Task<Result> UpdateUserSetting([FromBody] UserSettingDto userSettingDto)
    {
        await userService.UpdateUserSettingAsync(userSettingDto);
        return Result.Success();
    }
}