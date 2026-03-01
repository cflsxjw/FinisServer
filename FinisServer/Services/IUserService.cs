using FinisServer.Models;
using FinisServer.Models.Dtos;

namespace FinisServer.Services;

public interface IUserService
{
    public Task<string> SayHelloAsync(int id);
    public Task RegisterAsync(UserRegisterDto userRegisterDto);
    public Task<string> LoginAsync(UserLoginDto userLoginDto);
}