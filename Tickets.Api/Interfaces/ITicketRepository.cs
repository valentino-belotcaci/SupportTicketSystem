using Tickets.Api.Models;

namespace Tickets.Api.Interfaces
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync();
    }
}