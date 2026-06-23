using System.ComponentModel.DataAnnotations;
using Tickets.Api.Enums;

namespace Tickets.Api.Dtos.Ticket
{
    public class TicketDto
    {
        public Guid Id {    //property declaration ( field with getter and setter )
            get;
            set;
        } = Guid.NewGuid();
        
        [Required]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; }

        [Required]
        public TicketPriority Priority { get; set; }

        [Required]
        public TicketCategory Category { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;
    }
}