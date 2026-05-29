using FinisServer.Models;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpPost("article/block/{id:int}")]
    public async Task<Result> BlockArticleAsync(int id)
    {
        await adminService.BlockArticleAsync(id);
        return Result.Success();
    }
    
    [HttpPost("article/unblock/{id:int}")]
    public async Task<Result> UnBlockArticleAsync(int id)
    {
        await adminService.UnBlockArticleAsync(id);
        return Result.Success();
    }
}