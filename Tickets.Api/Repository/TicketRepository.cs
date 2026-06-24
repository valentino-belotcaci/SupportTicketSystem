using Tickets.Api.Data;
using Tickets.Api.Interfaces;
using Tickets.Api.Models;
using Microsoft.EntityFrameworkCore;
using Tickets.Api.Dtos.Ticket;

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

        public async Task<Ticket?> GetByIdAsync(Guid id)
        {
            return await _context.Tickets.FindAsync(id);
        }

        public async Task<Ticket> CreateAsync(Ticket ticketModel)
        {
            _context.Tickets.Add(ticketModel);
            await _context.SaveChangesAsync();
            return ticketModel;
        }

        public async Task<Ticket?> UpdateAsync(Guid id, UpdateTicketDto request)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (existingTicket == null)
                return null;

            existingTicket.Title = request.Title;
            existingTicket.Description = request.Description;
            existingTicket.Priority = request.Priority;
            existingTicket.Category = request.Category;
            existingTicket.AssignedTo = request.AssignedTo;
            existingTicket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingTicket;
        }

        public async Task<Ticket?> DeleteAsync(Guid id)
        {
            var stockModel = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            
            if (stockModel == null)
                return null;

            _context.Tickets.Remove(stockModel);

            await _context.SaveChangesAsync();

            return stockModel;
        }
    }
}