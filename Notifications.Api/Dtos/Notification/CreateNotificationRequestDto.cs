using System.ComponentModel.DataAnnotations;
using Notifications.Api.Enums;

namespace Notifications.Api.Dtos
{
    public class CreateNotificationRequestDto
    {
        
        [Required]
        public Guid TicketId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}