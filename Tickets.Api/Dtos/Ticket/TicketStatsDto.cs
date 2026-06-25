namespace Tickets.Api.Dtos.Ticket
{
    public class TicketStatsDto
    {
        public List<TicketStatusCountDto> ByStatus { get; set; } = new();
        public List<TicketPriorityCountDto> ByPriority { get; set; } = new();
    }
}