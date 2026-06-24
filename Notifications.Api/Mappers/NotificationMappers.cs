
using Notifications.Api.Models;
using Notifications.Api.Dtos;

namespace Notifications.Api.Mappers
{
    public static class NotificationMappers
    {
        public static NotificationDto ToNotificationDto(this Notification notificationModel)
        {
            return new NotificationDto
            {
                Id = notificationModel.Id,
                TicketId = notificationModel.TicketId,
                Type = notificationModel.Type,
                Message = notificationModel.Message,
                SentAt = notificationModel.SentAt,
            };
        }
        public static Notification ToNotificationFromCreateDto(this CreateNotificationRequestDto notificationDto){
            return new Notification{
                TicketId = notificationDto.TicketId,
                Type = notificationDto.Type,
                Message = notificationDto.Message,
            };
        }
    }
}
