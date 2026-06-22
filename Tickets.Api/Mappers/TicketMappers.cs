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
                Id = ticketModel.Id,
                Title = ticketModel.Title,
                Description = ticketModel.Description,
                Priority = ticketModel.Priority,
                Category = ticketModel.Category,
                CreatedBy = ticketModel.CreatedBy
            };
        }

        public static Ticket ToTicketFromCreateDto(this CreateTicketRequestDto ticketDto){
            return new Ticket{
                Title = ticketDto.Title,
                Description = ticketDto.Description,
                Priority = ticketDto.Priority,
                Category = ticketDto.Category,
                CreatedBy = ticketDto.CreatedBy
            };
        }
    }
}