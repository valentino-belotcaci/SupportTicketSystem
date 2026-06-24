using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Data;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Mappers;
using Tickets.Api.Enums;
using Tickets.Api.Queries;
using Microsoft.EntityFrameworkCore;
using Tickets.Api.Interfaces;// to use async

namespace Tickets.Api.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ITicketRepository _ticketRepo;
        public TicketController(ApplicationDBContext context, ITicketRepository ticketRepo) {
            
            _ticketRepo = ticketRepo;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TicketQuery query)
{
            var tickets = await _ticketRepo.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(query.Category) &&
                Enum.TryParse<TicketCategory>(query.Category, true, out var category))
                tickets = tickets.Where(t => t.Category == category).ToList();

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                Enum.TryParse<TicketStatus>(query.Status, true, out var status))
                tickets = tickets.Where(t => t.Status == status).ToList();

            if (!string.IsNullOrWhiteSpace(query.Priority) &&
                Enum.TryParse<TicketPriority>(query.Priority, true, out var priority))
                tickets = tickets.Where(t => t.Priority == priority).ToList();

            var result = tickets.Select(t => t.ToTicketDto()).ToList();

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id){
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequestDto ticketDto) {

            var ticketModel = ticketDto.ToTicketFromCreateDto();
            _context.Tickets.Add(ticketModel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetById), //execute getById method
                new { id = ticketModel.Id}, //pass this new object into the id of the getById method
                ticketModel.ToTicketDto()  //then return into the form of ticketDto
            );
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusDto request)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            if (request.Status <= ticket.Status)
                return BadRequest("Status cannot be moved backwards."); 

            ticket.Status = request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket([FromRoute] Guid id, [FromBody] AssignTicketDto request)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound();


            ticket.AssignedTo = request.AssignedTo;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPut("{id}")] 
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTicketDto request)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if(ticket == null)
                return NotFound();
            
            ticket.Title = request.Title;
            ticket.Description = request.Description;
            ticket.Priority = request.Priority;
            ticket.Category = request.Category;
            ticket.AssignedTo = request.AssignedTo;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();//send to db

            return Ok(ticket.ToTicketDto());
        }

        [HttpDelete("{id}")] 
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if(ticket == null)
                return NotFound();

            _context.Tickets.Remove(ticket);

            await _context.SaveChangesAsync();

            return NoContent();//success
        }
    }
}