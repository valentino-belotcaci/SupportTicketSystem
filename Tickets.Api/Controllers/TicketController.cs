using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Data;
using Tickets.Api.Mappers;

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
        public IActionResult GetAll(){
            var tickets = _context.Tickets.ToList().Select(//select works as a mapper in js
                s => s.ToTicketDto()
                );

            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] Guid id){
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }
    }
}