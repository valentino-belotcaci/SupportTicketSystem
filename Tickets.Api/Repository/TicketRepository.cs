using Tickets.Api.Data;
using Tickets.Api.Interfaces;
using Tickets.Api.Models;
using Microsoft.EntityFrameworkCore;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Services;
using Tickets.Api.Messages;

namespace Tickets.Api.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly RabbitMQPublisher _publisher;
        private readonly ILogger<TicketRepository> _logger;
        public TicketRepository(ApplicationDBContext context, RabbitMQPublisher publisher, ILogger<TicketRepository> logger)//dependency injection
        {
            _context = context;
            _publisher = publisher;//inject Ipublisher into the repository 
            _logger = logger;   
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

            try{
                _publisher.Publish(new TicketEventMessage
                {
                    TicketId = ticketModel.Id,
                    Type = "Created",
                    Message = $"Ticket '{ticketModel.Title}' was created by '{ticketModel.CreatedBy}'"
                });
            }catch(Exception ex){
                _logger.LogError(ex, "Failed to publish event for ticket {TicketId}", ticketModel.Id);
            }

            return ticketModel;
        }

        public async Task<Ticket?> UpdateStatusAsync(Guid id, UpdateStatusDto request)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (existingTicket == null)
                return null;

            var oldStatus = existingTicket.Status;

            existingTicket.Status = request.Status;
            existingTicket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try{
                _publisher.Publish(new TicketEventMessage
                {
                    TicketId = existingTicket.Id,
                    Type = "StatusChanged",
                    Message = $"Ticket '{existingTicket.Title}' changed status from  '{oldStatus}' to '{request.Status}'"
                });
            }catch(Exception ex){
                _logger.LogError(ex, "Failed to publish event for ticket {TicketId}", existingTicket.Id);
            }

            return existingTicket;
        }

        public async Task<Ticket?> AssignTicketAsync(Guid id, AssignTicketDto request)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (existingTicket == null)
                return null;

            var oldAssignment = existingTicket.AssignedTo;

            existingTicket.AssignedTo = request.AssignedTo;
            existingTicket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try{
                _publisher.Publish(new TicketEventMessage
                {
                    TicketId = existingTicket.Id,
                    Type = "StatusChanged",
                    Message = $"Ticket '{existingTicket.Title}' changed Assignment from  '{oldAssignment}' to '{request.AssignedTo}'"
                });
            }catch(Exception ex){
                _logger.LogError(ex, "Failed to publish event for ticket {TicketId}", existingTicket.Id);
            }

            return existingTicket;
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
            var ticketModel = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            
            if (ticketModel == null)
                return null;

            _context.Tickets.Remove(ticketModel);

            await _context.SaveChangesAsync();

            return ticketModel;
        }

        public async Task<List<TicketStatusCountDto>> GetTicketCountsGroupedByStatusAsync()
        {
            var result = await _context.Tickets
                .GroupBy(ticket => ticket.Status)
                .Select(group => new TicketStatusCountDto
                {
                    Status = group.Key,
                    Count = group.Count()
                })
                .ToListAsync();

            return result;
        }
    }
}