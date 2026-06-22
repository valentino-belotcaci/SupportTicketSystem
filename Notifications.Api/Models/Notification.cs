using System;
using Notifications.Api.Enums;

namespace Notifications.Api.Models
{
    public class Notification
    {

        public Guid Id{
            get;
            set;
        } = Guid.NewGuid();

        public Guid TicketId{// plain Guid, not a FK - different DB
            get;
            set;
        }

        public NotificationType Type {get; set;} = NotificationType.Created;

        public string Message {get; set;} = string.Empty;

        public DateTime SentAt {get; set;} = DateTime.UtcNow;

    }
}