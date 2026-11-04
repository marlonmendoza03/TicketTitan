using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketTitan.DTO;
using TicketTitan.Interfaces;

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _ticketService.GetAllTicketsAsync();
            return Ok(tickets);
        }

        [Authorize]
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated");
            }

            var result = await _ticketService.CreateTicketAsync(dto, userId);

            if(!result.IsSuccess || result.Value is null)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetTicketById), new { id = result.Value.Id }, result.Value);
        }

        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var updated = await _ticketService.UpdateStatusAsync(id, dto.Status, userId);
            if (!updated.IsSuccess)
            {
                switch(updated.ErrorMessage)
                {
                    case "Ticket not found":
                        return StatusCode(StatusCodes.Status404NotFound, updated.ErrorMessage);
                    case "User is not the owner of this ticket":
                        return StatusCode(StatusCodes.Status403Forbidden, updated.ErrorMessage);
                    default:
                        return BadRequest(updated.ErrorMessage);
                }
            }
            return NoContent();
        }
    }
}
