using FinisServer.Configurations.Database;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinisServer.Services.Impl;

public class NotificationService(FinisDbContext finisDbContext, IFinisHttpContext finisHttpContext) : INotificationService
{
    public async Task NotificationPublishAsync(NotificationPublishDto notificationPublishDto)
    {
        var newNotification = new SiteNotification
        {
            Title = notificationPublishDto.Title,
            Content = notificationPublishDto.Content,
            AuthorId = finisHttpContext.GetRequestUserId() ?? throw new BusinessException("用户状态异常")
        };
        finisDbContext.SiteNotifications.Add(newNotification);
        await finisDbContext.SaveChangesAsync();
    }
    public async Task<List<NotificationInfoDto>> GetNotificationsAsync()
    {
        return await finisDbContext.SiteNotifications
            .OrderByDescending(n => n.CreatedTimeOffset)
            .Take(30)
            .Select(n => new NotificationInfoDto(
                Id: n.Id,
                Content: n.Content,
                Title: n.Title,
                CreateOn: n.CreatedTimeOffset
            ))
            .ToListAsync();
    }
    public async Task NotificationDeleteAsync(int notificationId)
    {
        var notification = await finisDbContext.SiteNotifications
                                .Where(n => n.Id == notificationId)
                                .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("公告不存在");
        
        finisDbContext.SiteNotifications.Remove(notification);
        await finisDbContext.SaveChangesAsync();
    }
}