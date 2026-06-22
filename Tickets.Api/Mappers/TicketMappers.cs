using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Models;

namespace Tickets.Api.Mappers
{
    public static class TicketMappers
    {
        public static TicketDto ToTicketDto(this Ticket ticketModel) {
            return new TicketDto{
                Title = ticketModel.Title,
                Description = ticketModel.Description,
                Priority = ticketModel.Priority,
                Category = ticketModel.Category,
                CreatedBy = ticketModel.CreatedBy
            };
        }
    }
}