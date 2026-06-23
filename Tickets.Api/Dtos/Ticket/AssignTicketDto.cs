using System.ComponentModel.DataAnnotations;

namespace Tickets.Api.Dtos.Ticket
{
    public class AssignTicketDto
    {
        [Required]
        public string AssignedTo { get; set; } = string.Empty;
    }
}