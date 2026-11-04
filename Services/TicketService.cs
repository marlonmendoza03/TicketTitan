using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TicketTitan.Common;
using TicketTitan.Data;
using TicketTitan.DTO;
using TicketTitan.Hubs;
using TicketTitan.Interfaces;
using TicketTitan.Models;

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
                t.CreatedAt,
                t.CreatedByUserId
            ))
            .ToListAsync();
        }

        public async Task<TicketResponseDto?> GetTicketByIdAsync(int id)
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
                ticket.CreatedAt,
                ticket.CreatedByUserId
            );
        }

        public async Task<ServiceResult<TicketResponseDto>> CreateTicketAsync(CreateTicketDto dto, string userId)
        {
            var ticket = new Ticket{
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Priority = dto.Priority,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
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
                ticket.CreatedAt,
                ticket.CreatedByUserId
            );

            await _hubContext.Clients.All.SendAsync("TicketCreated", response);

            if(ticket.Priority == 4)
            {
                var alert = new TicketNotificationDto(ticket.Id, ticket.Title, $"CRITICAL TICKET CREATED: #{ticket.Id} - {ticket.Title}", "HighPriorityAlert", DateTime.UtcNow);
                await _hubContext.Clients.All.SendAsync("ReceiveHighPriorityNotification", alert);
            }

            return ServiceResult<TicketResponseDto>.Success(response);
        }

        public async Task<ServiceResult<bool>> UpdateStatusAsync(int id, string newStatus, string UserId) { 
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return ServiceResult<bool>.Failure("Ticket not found");

            if (ticket.CreatedByUserId != UserId)
            {
                return ServiceResult<bool>.Failure("User is not the owner of this ticket");
            }

            string oldStatus = ticket.Status;
            ticket.Status = newStatus;
            ticket.UpdatedByUserId = UserId;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("TicketStatusUpdated", new { TicketId = ticket.Id, NewStatus = newStatus });

            if(newStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase) && !oldStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                var notification = new TicketNotificationDto(ticket.Id, ticket.Title, $"Ticket #${ticket.Id} has been marked as Resolved.", "TicketResolved", DateTime.UtcNow);
                await _hubContext.Clients.All.SendAsync("ReceiveTicketResolvedNotification", notification);
            }

            return ServiceResult<bool>.Success(true);
        }

    }
}
