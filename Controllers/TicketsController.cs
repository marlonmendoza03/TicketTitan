using Microsoft.AspNetCore.Mvc;
using TicketTitan.DTO;
using TicketTitan.Services;

namespace TicketTitan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets() {
            var tickets = await _ticketService.GetAllTicketsAsync();
            return Ok(tickets);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var result = await _ticketService.CreateTicketAsync(dto);
            return CreatedAtAction(nameof(GetTicketById), new { id = result.Id }, result);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusDto dto)
        {
            var updated = await _ticketService.UpdateStatusAsync(id, dto.Status);
            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
