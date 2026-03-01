using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Services;
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
    public async Task<Result<string>> Login([FromBody] UserLoginDto userLoginDto)
    {
        var token = await userService.LoginAsync(userLoginDto);
        return Result<string>.Success(token, "登录成功");
    }
    
    
}