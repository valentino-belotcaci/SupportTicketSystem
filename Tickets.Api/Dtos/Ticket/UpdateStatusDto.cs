using Tickets.Api.Enums;

namespace Tickets.Api.Dtos.Ticket
{
    public class UpdateStatusDto
    {
        public TicketStatus Status { get; set; }
    }
}