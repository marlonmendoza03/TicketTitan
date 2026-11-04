using TicketTitan.Common;
using TicketTitan.DTO;

namespace TicketTitan.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketResponseDto>> GetAllTicketsAsync();
        Task<TicketResponseDto?> GetTicketByIdAsync(int id);
        Task<ServiceResult<TicketResponseDto>> CreateTicketAsync(CreateTicketDto dto, string userId);
        Task<ServiceResult<bool>> UpdateStatusAsync(int id, string newStatus, string userId);
    }
}
