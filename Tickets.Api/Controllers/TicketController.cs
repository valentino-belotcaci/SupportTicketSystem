using Microsoft.AspNetCore.Mvc;
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
        private readonly ITicketRepository _ticketRepo;
        public TicketController(ITicketRepository ticketRepo) {
            
            _ticketRepo = ticketRepo;
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
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequestDto ticketDto) {

            var ticketModel = ticketDto.ToTicketFromCreateDto();
            
            await _ticketRepo.CreateAsync(ticketModel);

            return CreatedAtAction(
                nameof(GetById), //execute getById method
                new { id = ticketModel.Id}, //pass this new object into the id of the getById method
                ticketModel.ToTicketDto()  //then return into the form of ticketDto
            );
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusDto request)
        {
            var existing = await _ticketRepo.GetByIdAsync(id);

            if (existing == null)
                return NotFound();

            if (request.Status <= existing.Status)
                return BadRequest("Status cannot be moved backwards."); 
            
            var ticket = await _ticketRepo.UpdateStatusAsync(id, request);

            return Ok(ticket!.ToTicketDto());
        }

        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket([FromRoute] Guid id, [FromBody] AssignTicketDto request)
        {
            var ticket = await _ticketRepo.AssignTicketAsync(id, request);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }

        [HttpPut("{id}")] 
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTicketDto request)
        {
            var ticket = await _ticketRepo.UpdateAsync(id, request);

            if(ticket == null)
                return NotFound();
            

            return Ok(ticket.ToTicketDto());
        }

        [HttpDelete("{id}")] 
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var ticket = await _ticketRepo.DeleteAsync(id);

            if(ticket == null)
                return NotFound();


            return NoContent();//success
        }
    }
}