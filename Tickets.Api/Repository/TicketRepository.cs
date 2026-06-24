using Tickets.Api.Data;
using Tickets.Api.Interfaces;
using Tickets.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Api.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDBContext _context;
        public TicketRepository(ApplicationDBContext context)//dependency injection
        {
            _context = context;
        }
        public async Task<List<Ticket>> GetAllAsync()
        {
            return await _context.Tickets.ToListAsync();

        }
    }
}