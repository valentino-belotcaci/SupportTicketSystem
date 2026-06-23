using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Data;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Mappers;
using Tickets.Api.Enums;
using Tickets.Api.Queries;

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
        public IActionResult GetTickets([FromQuery] TicketQuery query)
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

            var result = tickets
                .Select(t => t.ToTicketDto())
                .ToList();

            return Ok(result);
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

        [HttpPatch]
    }
}