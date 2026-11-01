using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TicketTitan.Data;
using TicketTitan.DTO;
using TicketTitan.Hubs;
using TicketTitan.Models;
using TicketTitan.Validators;

namespace TicketTitan.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TicketHub> _hubContext;

        public TicketService(
            ApplicationDbContext context, 
            IHubContext<TicketHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<TicketResponseDto>> GetAllTicketsAsync()
        {
            return await _context.Tickets.Select(t => new TicketResponseDto
            (
                t.Id,
                t.Title,
                t.Description,
                t.Category,
                t.Status,
                t.Priority,
                t.CreatedAt
            ))
            .ToListAsync();
        }

        public async Task<TicketResponseDto> GetTicketByIdAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if(ticket == null) return null;

            return new TicketResponseDto(
                ticket.Id, 
                ticket.Title, 
                ticket.Description, 
                ticket.Category, 
                ticket.Status, 
                ticket.Priority, 
                ticket.CreatedAt
            );
        }

        public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto)
        {
            var ticket = new Ticket{
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Priority = dto.Priority,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var response = new TicketResponseDto(
                ticket.Id,
                ticket.Title,
                ticket.Description,
                ticket.Category,
                ticket.Status,
                ticket.Priority,
                ticket.CreatedAt
            );

            await _hubContext.Clients.All.SendAsync("TicketCreated", response);

            if(ticket.Priority == 4)
            {
                var alert = new TicketNotificationDto(ticket.Id, ticket.Title, $"CRITICAL TICKET CREATED: #{ticket.Id} - {ticket.Title}", "HighPriorityAlert", DateTime.UtcNow);
                await _hubContext.Clients.All.SendAsync("ReceiveHighPriorityNotification", alert);
            }

            return response;
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus) { 
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return false;

            string oldStatus = ticket.Status;
            ticket.Status = newStatus;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("TicketStatusUpdated", new { TicketId = ticket.Id, NewStatus = newStatus });

            if(newStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase) && !oldStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                var notification = new TicketNotificationDto(ticket.Id, ticket.Title, $"Ticket #${ticket.Id} has been marked as Resolved.", "TicketResolved", DateTime.UtcNow);
                await _hubContext.Clients.All.SendAsync("ReceiveTicketResolvedNotification", notification);
            }

            return true;
        }

    }
}
