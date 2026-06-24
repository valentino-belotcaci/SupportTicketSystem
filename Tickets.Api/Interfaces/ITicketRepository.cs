using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Models;

namespace Tickets.Api.Interfaces
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync();

        Task<Ticket?> GetByIdAsync(Guid id);

        Task<Ticket> CreateAsync(Ticket ticketModel);

        Task<Ticket?> UpdateAsync(Guid id, UpdateTicketDto ticketDto);

        Task<Ticket?> DeleteAsync(Guid id);
    }
}