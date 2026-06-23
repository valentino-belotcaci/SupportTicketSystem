using Tickets.Api.Enums;

namespace Tickets.Api.Models
{
    public class Ticket
    {

        /*
        ###EQUIVALENT IN JAVA###
        private UUID id;
        public UUID getId() { return id; }
        public void setId(UUID id) { this.id = id; }
        */
        public Guid Id {    //property declaration ( field with getter and setter )
            get;
            set;
        } = Guid.NewGuid();


        public string Title { get; set; } = string.Empty; // required field

        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Open; //always Open at creation
        public TicketPriority Priority { get; set; } // user prov this
        public TicketCategory Category { get; set; } // user prob this

        public string CreatedBy { get; set; } = string.Empty;

        public string? AssignedTo { get; set; } // optional field

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;//UtcNow is timezone-independent, app runs on servers in different countries

        public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
    }
}