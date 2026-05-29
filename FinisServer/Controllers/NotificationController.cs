using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Exceptions;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    [ApiController]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        [HttpGet("fetch")]
        public async Task<Result<List<NotificationInfoDto>>> GetNotificationsAsync()
        {
            var result = await notificationService.GetNotificationsAsync();
            return Result<List<NotificationInfoDto>>.Success(result);
        }

        [HttpPost("post")]
        public async Task<Result> NotificationPublishAsync( [FromBody] NotificationPublishDto notificationPublishDto)
        {
            await notificationService.NotificationPublishAsync(notificationPublishDto);
            return Result.Success();
        }
        [HttpPost("delete/{id:int}")]
        public async Task<Result> NotificationDeleteAsync(int id)
        {
            await notificationService.NotificationDeleteAsync(id);
            return Result.Success();
        }
    }
}
