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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TicketRepository> _logger;
        public TicketRepository(ApplicationDBContext context,IHttpClientFactory httpClientFactory, ILogger<TicketRepository> logger)//dependency injection
        {
            _context = context;
            _httpClientFactory = httpClientFactory;//inject IHttpClientFactory into the repository
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

            //notify Notifications.Api
            var client = _httpClientFactory.CreateClient();

            var payload = new{

                ticketId = ticketModel.Id,
                type = 0, //0=just created
                message = $"Ticket '{ticketModel.Title}' was created"
            };

            //send http request with payload in the body
            //then notifications.api receives it
            //the controller sees POST api/notifications and calls this method that actually creates the notification

            try
            {
                await client.PostAsJsonAsync(
                    "http://localhost:5201/api/notifications",
                    payload
                );
            }
            catch (Exception ex)
            {
                //notification failed, don't fail the ticket creation, we just log it
                _logger.LogError(ex, "Failed to notify Notifications.Api for ticket {TicketId}", ticketModel.Id);

                //in future ill use message queue (RabbitMQ) to guarantee delivery
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

            var client = _httpClientFactory.CreateClient();

            var payload = new{
                ticketId = existingTicket.Id,
                type = 1,
                message = $" Ticket '{existingTicket.Title}' changed status from '{oldStatus}' to '{request.Status}'"
            };

            try {
                await client.PostAsJsonAsync(
                    "http://localhost:5201/api/notifications",
                    payload
                );
            }catch(Exception ex) {
                _logger.LogError(ex, "Failed to notify Notifications.Api for ticket {TicketId}", existingTicket.Id);

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

            var client = _httpClientFactory.CreateClient();

            var payload = new{
                ticketId = existingTicket.Id,
                type = 2,
                message = $" Ticket '{existingTicket.Title}' changed Assignment from '{oldAssignment}' to '{request.AssignedTo}'"
            };

            try {
                await client.PostAsJsonAsync(
                    "http://localhost:5201/api/notifications",
                    payload
                );
            }catch(Exception ex) {
                _logger.LogError(ex, "Failed to notify Notifications.Api for ticket {TicketId}", existingTicket.Id);

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
    }
}