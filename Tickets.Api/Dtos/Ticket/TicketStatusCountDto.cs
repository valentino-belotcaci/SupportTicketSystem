using Tickets.Api.Enums;

namespace Tickets.Api.Dtos.Ticket
{
    public class TicketStatusCountDto
{
    public TicketStatus Status { get; set; }
    public int Count { get; set; }
}
}