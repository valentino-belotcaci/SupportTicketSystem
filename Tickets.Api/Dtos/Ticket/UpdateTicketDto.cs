using Tickets.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Tickets.Api.Dtos.Ticket
{
    public class UpdateTicketDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TicketPriority Priority { get; set; }

        [Required]
        public TicketCategory Category { get; set; }

        public string? AssignedTo { get; set; }
    }
}