using Tickets.Api.Enums;

namespace Tickets.Api.Dtos.Ticket
{
    public class UpdateTicketDto
    {
        public string Title { get; set; } = string.Empty; // required field

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } // user prov this
        public TicketCategory Category { get; set; } // user prob this

        public string CreatedBy { get; set; } = string.Empty;
    }
}