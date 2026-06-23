using Tickets.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Tickets.Api.Dtos.Ticket
{
    public class UpdateStatusDto
    {
        [Required]
        public TicketStatus Status { get; set; }
    }
}