using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Data;

namespace Tickets.Api.Controllers
{
    [Route("api/ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public TicketController(ApplicationDBContext context) {

            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll(){
            var tickets = _context.Tickets.ToList();

            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] Guid id){
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }
    }
}