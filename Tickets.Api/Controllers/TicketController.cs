using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Data;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Mappers;
using Tickets.Api.Enums;

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
        public IActionResult GetTickets(
            [FromQuery] string? category,
            [FromQuery] string? status,
            [FromQuery] string? priority)
        {
            var query = _context.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) &&
                Enum.TryParse<TicketCategory>(category, true, out var parsedCategory))
            {
                query = query.Where(t => t.Category == parsedCategory);
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(t => t.Status == parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(priority) &&
                Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority))
            {
                query = query.Where(t => t.Priority == parsedPriority);
            }

            var tickets = query
                .Select(t => t.ToTicketDto())
                .ToList();

            return Ok(tickets);
        }


        [HttpGet("{id}")]
        public IActionResult GetTicketsById([FromRoute] Guid id){
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPost]
        public IActionResult create([FromBody] CreateTicketRequestDto ticketDto) {

            var ticketModel = ticketDto.ToTicketFromCreateDto();
            _context.Tickets.Add(ticketModel);
            _context.SaveChanges();
            return CreatedAtAction(
                nameof(GetTicketsById), //execute getById method
                new { id = ticketModel.Id}, //pass this new object into the id of the getById method
                ticketModel.ToTicketDto()  //then return into the form of ticketDto
            );
        }
    }
}