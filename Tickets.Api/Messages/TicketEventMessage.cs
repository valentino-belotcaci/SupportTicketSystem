namespace Tickets.Api.Messages
{
    public class TicketEventMessage
    {
        public Guid TicketId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}