using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tickets.Api.Dtos.Ticket
{
    public class AssignTicketDto
    {
        public string AssignedTo {get; set;} = string.Empty;
    }
}