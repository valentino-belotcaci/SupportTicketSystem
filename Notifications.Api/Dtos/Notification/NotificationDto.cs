using Notifications.Api.Enums;

namespace Notifications.Api.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}