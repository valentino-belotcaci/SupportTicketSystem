using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Mappers;
using Tickets.Api.Enums;
using Tickets.Api.Queries;
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

        /// <summary>Returns all tickets with optional filtering</summary>
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

        /// <summary>Returns a single ticket by ID</summary>
        /// <param name="id">The ticket GUID</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id){
            var ticket = await _ticketRepo.GetByIdAsync(id);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }

        /// <summary>Creates a new support ticket</summary>
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

        /// <summary>Updates the status of a ticket</summary>
        /// <param name="id">The ticket GUID</param>
        /// <param name="request">The new status</param>
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

        /// <summary>Assigns a ticket to a team member</summary>
        /// <param name="id">The ticket GUID</param>
        /// <param name="request">The new status</param>
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket([FromRoute] Guid id, [FromBody] AssignTicketDto request)
        {
            var ticket = await _ticketRepo.AssignTicketAsync(id, request);

            if (ticket == null)
                return NotFound();

            return Ok(ticket.ToTicketDto());
        }

        /// <summary>Updates ticket details</summary>
        /// <param name="id">The ticket GUID</param>
        /// <param name="request">The new status</param>
        [HttpPut("{id}")] 
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTicketDto request)
        {
            var ticket = await _ticketRepo.UpdateAsync(id, request);

            if(ticket == null)
                return NotFound();
            

            return Ok(ticket.ToTicketDto());
        }

        /// <summary>Deletes a ticket</summary>
        /// <param name="id">The ticket GUID</param>
        [HttpDelete("{id}")] 
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var ticket = await _ticketRepo.DeleteAsync(id);

            if(ticket == null)
                return NotFound();


            return NoContent();//success
        }

        /// <summary>Returns number of tickets assigned to each status</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetTicketCountsGroupedByStatus(){

            var result = await _ticketRepo.GetTicketCountsGroupedByStatusAsync();

            return Ok(result);

        }

        [HttpGet("priority")]
        public async Task<IActionResult> GetTicketCountsGroupedByPriority(){

            var result = await _ticketRepo.GetTicketCountsGroupedByPriorityAsync();

            return Ok(result);

        }
    }
}