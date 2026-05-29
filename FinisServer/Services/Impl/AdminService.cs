using FinisServer.Configurations.Database;
using FinisServer.Models.Enums;
using FinisServer.Models.Exceptions;
using FinisServer.Services.Impl;
using Microsoft.EntityFrameworkCore;

namespace FinisServer.Services;

public class AdminService(FinisDbContext finisDbContext, IFinisHttpContext finisHttpContext) : IAdminService
{
    public async Task BlockArticleAsync(int id)
    {
        var userId = finisHttpContext.GetRequestUserId() ?? throw new OperationNotAllowedException("没有合适的权限");
        var user = await finisDbContext.Users.Where(u => u.Id == userId).FirstOrDefaultAsync() ?? throw new OperationNotAllowedException("没有合适的权限");
        if (!(user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin))
        {
            throw new OperationNotAllowedException("没有合适的权限");
        }
        var article = await finisDbContext.Articles.Where(a => a.Id == id).FirstOrDefaultAsync() ?? throw new BusinessException("请求的文章不存在");
        article.IsDeletedByAdmin = true;
        await finisDbContext.SaveChangesAsync();
    }
    public async Task UnBlockArticleAsync(int id)
    {
        var userId = finisHttpContext.GetRequestUserId() ?? throw new OperationNotAllowedException("没有合适的权限");
        var user = await finisDbContext.Users.Where(u => u.Id == userId).FirstOrDefaultAsync() ?? throw new OperationNotAllowedException("没有合适的权限");
        if (!(user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin))
        {
            throw new OperationNotAllowedException("没有合适的权限");
        }
        var article = await finisDbContext.Articles.Where(a => a.Id == id).FirstOrDefaultAsync() ?? throw new BusinessException("请求的文章不存在");
        article.IsDeletedByAdmin = false;
        await finisDbContext.SaveChangesAsync();
    }
}