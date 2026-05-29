using FinisServer.Configurations.Database;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using FinisServer.Models.Exceptions;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace FinisServer.Services.Impl;

public class ReportService(FinisDbContext finisDbContext, IFinisHttpContext finisHttpContext, IArticleService articleService) : IReportService
{
    public async Task SubmitReportAsync(ReportDto reportDto)
    {
        var userid = finisHttpContext.GetRequestUserId() ?? throw new BusinessException("用户不存在");
        
        var newReport = new ReportRecord
        {
            ReporterId = userid,
            TargetIdentifierId = reportDto.TargetIdentifierId,
            ReportType = reportDto.ReportType,
            Reason = reportDto.Reason
        };
        finisDbContext.Add(newReport);
        await finisDbContext.SaveChangesAsync();
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task ReportComplete(int id, int actionType)
    {
        int userid = finisHttpContext.GetRequestUserId() ?? throw new BusinessException("用户不存在");
        var report = await finisDbContext.ReportRecords
                            .Where(r => r.Id == id)
                            .FirstOrDefaultAsync() ?? throw new BusinessException("举报不存在");
        report.AdminId = userid;
        switch (actionType)
        {
            case 0:
                break;
            case 1:
                if (report.ReportType == ReportType.Article)
                {
                    var article = await finisDbContext.Articles
                        .Where(a => a.Id == report.TargetIdentifierId).FirstOrDefaultAsync() 
                        ?? throw new BusinessException("请求异常");
                    article.IsDeletedByAdmin = true;
                }
                if (report.ReportType == ReportType.Comment)
                {
                    var comment = await finisDbContext.Comments
                        .Where(c => c.Id == report.TargetIdentifierId).FirstOrDefaultAsync() 
                        ?? throw new BusinessException("请求异常");
                    var article = await finisDbContext.Articles
                        .Where(a => a.Id == comment.ArticleId).FirstOrDefaultAsync() 
                        ?? throw new BusinessException("请求异常");
                    finisDbContext.Comments.Remove(comment);
                    article.CommentCount--;
                    await finisDbContext.SaveChangesAsync();
                }
                break;
            default:
                break;
        }
        report.IsDone = true;
        await finisDbContext.SaveChangesAsync();
    }
    public async Task<List<ReportInfoDto>> GetReportsAsync()
    {
        var userid = finisHttpContext.GetRequestUserId() ?? throw new BusinessException("用户不存在");
        
        return await finisDbContext.ReportRecords
            .Where(r => !r.IsDone)
            .Select(r => new ReportInfoDto(
                r.Id,
                r.ReportType,
                r.Reason,
                r.TargetIdentifierId))
            .ToListAsync();
    }
}