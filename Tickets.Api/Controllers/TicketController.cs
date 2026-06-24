using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Data;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Mappers;
using Tickets.Api.Enums;
using Tickets.Api.Queries;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Api.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public TicketController(ApplicationDBContext context) {

            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TicketQuery query)
        {
            var tickets = _context.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Category) &&
                Enum.TryParse<TicketCategory>(query.Category, true, out var category))
            {
                tickets = tickets.Where(t => t.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                Enum.TryParse<TicketStatus>(query.Status, true, out var status))
            {
                tickets = tickets.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(query.Priority) &&
                Enum.TryParse<TicketPriority>(query.Priority, true, out var priority))
            {
                tickets = tickets.Where(t => t.Priority == priority);
            }

            var result = await tickets
                .Select(t => t.ToTicketDto())
                .ToListAsync();

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
        public IActionResult Create([FromBody] CreateTicketRequestDto ticketDto) {

            var ticketModel = ticketDto.ToTicketFromCreateDto();
            _context.Tickets.Add(ticketModel);
            _context.SaveChanges();
            return CreatedAtAction(
                nameof(GetById), //execute getById method
                new { id = ticketModel.Id}, //pass this new object into the id of the getById method
                ticketModel.ToTicketDto()  //then return into the form of ticketDto
            );
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusDto request)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            if (request.Status <= ticket.Status)
                return BadRequest("Status cannot be moved backwards."); 

            ticket.Status = request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPatch("{id}/assign")]
        public IActionResult AssignTicket([FromRoute] Guid id, [FromBody] AssignTicketDto request)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);

            if (ticket == null)
                return NotFound();


            ticket.AssignedTo = request.AssignedTo;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPut("{id}")] 
        public IActionResult Update([FromRoute] Guid id, [FromBody] UpdateTicketDto request)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);

            if(ticket == null)
                return NotFound();
            
            ticket.Title = request.Title;
            ticket.Description = request.Description;
            ticket.Priority = request.Priority;
            ticket.Category = request.Category;
            ticket.AssignedTo = request.AssignedTo;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();//send to db

            return Ok(ticket.ToTicketDto());
        }

        [HttpDelete("{id}")] 
        public IActionResult Delete([FromRoute] Guid id)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);

            if(ticket == null)
                return NotFound();

            _context.Tickets.Remove(ticket);

            _context.SaveChanges();

            return NoContent();//success
        }
    }
}