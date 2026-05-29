using FinisServer.Models.Dtos;

namespace FinisServer.Services;

public interface INotificationService
{
    public Task NotificationPublishAsync(NotificationPublishDto notificationPublishDto);
    public Task NotificationDeleteAsync(int notificationId);
    public Task<List<NotificationInfoDto>> GetNotificationsAsync();
}